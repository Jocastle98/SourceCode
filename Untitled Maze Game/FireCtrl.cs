using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireCtrl : MonoBehaviour
{
    public GameObject bullet;
    public Transform firePos;

    public AudioClip fireSfx;
    public AudioSource source;

    //private Animator animator;
    private PlayerCtrl player;


    private void Start()
    {
        source = GetComponent<AudioSource>();
        //animator = GetComponent<Animator>();
        player = GetComponent<PlayerCtrl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.isDie)
            return;


        if(Input.GetMouseButtonDown(0))
        {
            Fire();
            //animator.SetTrigger("Hit");
        }
    }
    void Fire()
    {
        CreateBullet();
        source.PlayOneShot(fireSfx, 0.9f);
    }
    void CreateBullet()
    {
        Instantiate(bullet, firePos.position, firePos.rotation);
    }
}
