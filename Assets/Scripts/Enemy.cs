using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour {

	public int health = 100;
	public float speed = 5f;
	public float stopDistance = 5f;
	private Transform target;
	public int attackDamage = 1;
	public float attackRange = 1f;
	public LayerMask attackMask;
	public Slider slider;

	void Start ()
    {
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
			transform.position = Vector2.MoveTowards(transform.position, target.position, speed = Time.deltaTime);
	}

    }

	public void SetMaxHealth (int health)	
	{
		slider.maxValue = health;
		slider.value = health;
	}
}
