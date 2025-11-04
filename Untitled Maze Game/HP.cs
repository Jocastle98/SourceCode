using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HP : MonoBehaviour
{


    private Transform tr;
  


    private Animator animator;

    private int hp = 100;
    public bool isDie = false;

    private int initHp;
    public Image imgHpbar;

    GameManager gameMgr;


    // Start is called before the first frame update
    void Start()
    {

        initHp = hp;

        tr = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        gameMgr = GameObject.Find("GameManager").GetComponent<GameManager>();

 
    }

    // Update is called once per frame
    void Update()
    {
       

       
    }



    private void OnTriggerEnter(Collider coll)
    {
        if (isDie)
            return;

        if (coll.gameObject.tag == "PUNCH")
        {
            hp -= 10;

            imgHpbar.fillAmount = (float)hp / (float)initHp;
            Debug.Log("Player HP = " + hp);
            if (hp <= 0)
            {
                PlayerDie();
            }
            else
            {
                //맞는 애니메이션 아직없름
                //animator.SetTrigger("Hit");
            }
        }


        //체력아이템
        
        else if (coll.gameObject.tag == "HPPill")
        {
            Heal heal = coll.gameObject.GetComponent<Heal>();

            if (heal != null)
            {
                hp += heal.healAmount;

                if (hp > 100)
                {
                    hp = 100;
                }
                imgHpbar.fillAmount = (float)hp / (float)initHp;
            }
            Destroy(coll.gameObject);
        }
    }


    void PlayerDie()
    {
        Debug.Log("Player Die!");
        //죽을때 애니메이션 아직 없음
        //animator.SetTrigger("Die");
        isDie = true;

        GameObject[] monsters = GameObject.FindGameObjectsWithTag("MONSTER");

        foreach (GameObject monster in monsters)
        {
            monster.SendMessage("OnPlayerDie", SendMessageOptions.DontRequireReceiver);
        }

        //gameMgr.isGameOver = true;
        GameManager.instance.isGameOver = true;
    }
}
