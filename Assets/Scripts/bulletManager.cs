using System.Collections;
using UnityEngine;


public class bulletManager : MonoBehaviour
{
    private Vector3 bulletDirection;
    private float bulletSpeed;
    IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        bulletSpeed = 1f;
    }
    
    void Update()
    {
        transform.Translate(bulletDirection * bulletSpeed * Time.deltaTime,Space.World);
        
        transform.GetChild(0).Rotate(Vector3.forward * 150 * Time.deltaTime);
    }

    public void GetTheCoordinates(Vector3 BulletDirection,float speed)
    {
        bulletDirection = BulletDirection;
        bulletSpeed = speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("enemy"))
        {
            if (other.GetComponent<zombieManager>().health == 0)
            {
                other.GetComponent<Animator>().SetBool("dead",true);
                Time.timeScale = 1f;
                PlayerManager.PlayerManagerInstance.blood.transform.position = transform.position;
                PlayerManager.PlayerManagerInstance.blood.Play();
            }
            
            PlayerManager.PlayerManagerInstance.followCam.gameObject.SetActive(false);
            
           
        }
        
        gameObject.SetActive(false);
    }
}
