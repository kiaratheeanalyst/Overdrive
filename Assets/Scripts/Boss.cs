using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    	public Transform player;
	public int health = 100;
	public float speed = 25f;
	public float stopDistance = 5f;
	private Transform target;
	public int attackDamage = 1;
	public float attackRange = 1f;
	public LayerMask attackMask;
	public Slider slider;
    	public LevelEndUIScript levelEndUIScript;

    	public bool isFlipped = false;

    	public void LookAtPlayer()
    	{
        Vector3 flipped = transform.localScale;
        flipped.z *= -1f;

        if (transform.position.x > player.position.x && isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = false;
        }
        else if (transform.position.x < player.position.x && !isFlipped)
        {
            transform.localScale = flipped;
            transform.Rotate(0f, 180f, 0f);
            isFlipped = true;
        }
    }

	void Start ()
    {
		speed = speed;
		target = GameObject.FindGameObjectWithTag("PlayerLayer").GetComponent<Transform>();
    }

	public void TakeDamage (int damage)
	{
		health -= damage;

		if (health <= 0)
		{
			Die();
		}
	}

	void Die ()
	{

		Destroy(gameObject);
		levelEndUIScript.BossOneGot();
		slider.value = 0;

	}

    private void Update()
    {
		slider.value = health;

		if (Vector2.Distance(transform.position, target.position) < stopDistance)
        {
		Vector3 pos = transform.position;
		Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
		if (colInfo != null)
		{
			colInfo.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
		}
	} else
	{
		Move();
	}

    }

	public void SetMaxHealth (int health)	
	{
		slider.maxValue = health;
		slider.value = health;
	}

	public void Move ()
	{
		transform.position = Vector2.MoveTowards(transform.position, target.position, speed = Time.deltaTime);
	}

}

