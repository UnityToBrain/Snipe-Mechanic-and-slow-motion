using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class PlayerManager : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
            }
            
            public void LerpTowards(CameraState target, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3( Mathf.Clamp(pitch , -50f,50f) , Mathf.Clamp(yaw,-50f,50f), 0f);
            }
        }

        const float k_MouseSensitivityMultiplier = 0.01f;

        CameraState m_TargetCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();
        
        [Header("Rotation Settings")]
        [Tooltip("Multiplier for the sensitivity of the rotation.")]
        public float mouseSensitivity = 60.0f;

        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY;
        
        //==================================== new scripts ===================================
        
        public static PlayerManager PlayerManagerInstance;
        public CinemachineVirtualCamera followCam;
        public bool disableCameraRotation,aimVisible,reload;
        
        [SerializeField] private GameObject aim,snipe,bulletInitialPos,bullet;
        public Animator snipeAnimator;
        [SerializeField] private LayerMask bulletTargetLayer;
        private Transform chosenEnemy;
        private Vector3 bulletDirection;
        private float BulletSpeed;
        private bool CamIsAlive,slowMotion;
        private Camera mainCam;
        public ParticleSystem blood;

        public GameObject[] PostProcessing;
       

        private void Start()
        {
            mainCam = Camera.main;
            snipeAnimator = snipe.GetComponent<Animator>();
            PlayerManagerInstance = this;

            reload = true;
        }

        void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
            invertY = true;
        }

        void Update()
        {
            if (!disableCameraRotation)
            {

                Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

                if (Physics.Raycast(ray,out RaycastHit hit,Mathf.Infinity,bulletTargetLayer))
                {

                    if (hit.collider.CompareTag("enemy"))
                    {
                        
                        chosenEnemy = hit.collider.transform;
                    }
                    else
                    {
                        chosenEnemy = null;
                    }
                    
                    bulletDirection = hit.point - bulletInitialPos.transform.position;
                 
                    
                }
                
               // Debug.DrawRay(transform.position * dir.y, ray.direction * dir.x , Color.green);
                
                if (IsLeftMouseButtonDown() && reload)
                {
                    snipe.transform.DOLocalMove(new Vector3(0.0299999993f, -0.129999995f, 0.50999999f), 0.5f)
                        .SetEase(Ease.InBack).OnComplete(ShowAim_HideSnipe);

                    reload = false;
                }
            
                if (IsLeftMouseButtonUp() && aimVisible)
                {
                    GameObject newBullet = Instantiate(bullet, bulletInitialPos.transform.position, Quaternion.identity);
                    newBullet.transform.rotation = mainCam.transform.rotation;
                    BulletSpeed = 1f;

                    if (chosenEnemy != null && chosenEnemy.GetComponent<zombieManager>().health > 0)
                    {
                        chosenEnemy.GetComponent<zombieManager>().health--;
                      
                        chosenEnemy.GetComponent<NavMeshAgent>().SetDestination(transform.position);
                    
                        chosenEnemy.GetComponent<Animator>().SetBool("run", chosenEnemy.GetComponent<zombieManager>().health > 0);

                        if ( chosenEnemy.GetComponent<zombieManager>().health == 0) // Active Slow Motion
                        {
                            chosenEnemy.GetComponent<NavMeshAgent>().enabled = false;
                            
                            followCam.gameObject.SetActive(true);
                            followCam.Follow = newBullet.transform;
                            followCam.LookAt = newBullet.transform;

                            Time.timeScale = 0.5f;
                            BulletSpeed = 0.1f;

                        }
                        else
                        {
                            blood.Play();
                            blood.transform.position = hit.point;
                        }

                    }

                    
                    newBullet.GetComponent<bulletManager>().GetTheCoordinates(bulletDirection,BulletSpeed);

                    Invoke("ShowSnipe_HideAim",0.2f);

                    aimVisible = false;
                }

                // Rotation
                if (IsCameraRotationAllowed())
                {
                    var mouseMovement = GetInputLookRotation() * k_MouseSensitivityMultiplier * mouseSensitivity;
                    if (invertY)
                        mouseMovement.y = -mouseMovement.y;

                    var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                    m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                    m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
                }
            
                var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
                m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, rotationLerpPct);

                m_InterpolatingCameraState.UpdateTransform(transform);
            }

            if (slowMotion)
                Invoke("ShowSnipe_HideAim",0.2f);
            

        }
        
        Vector2 GetInputLookRotation()
        {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        }

        bool IsCameraRotationAllowed()
        {
            return Input.GetMouseButton(0);
        }

        bool IsLeftMouseButtonDown()
        {
            return Input.GetMouseButtonDown(0);
        }

        bool IsLeftMouseButtonUp()
        {
            return Input.GetMouseButtonUp(0);
        }


        private void ShowAim_HideSnipe()
        {
            snipe.SetActive(false);
            aim.SetActive(true);
            aimVisible = true;
            PostProcessing[0].SetActive(true); // film grain On
        }

        private void ShowSnipe_HideAim()
        {
            aim.SetActive(false);
            PostProcessing[0].SetActive(false); // film grain Off
            
            if (!CinemachineCore.Instance.IsLive(followCam)) // no more slow motion
            {
                snipe.SetActive(true);
                snipe.transform.DOLocalMove(new Vector3(0.100000001f, -0.129999995f, 0.720000029f), 0.5f).SetEase(Ease.OutBack);
                snipeAnimator.SetBool("reload",true);
                PostProcessing[1].SetActive(true); // depth of field
                slowMotion = disableCameraRotation = false;
            }
            else
            {
                slowMotion = disableCameraRotation = true;
            }
         
        }
    }

