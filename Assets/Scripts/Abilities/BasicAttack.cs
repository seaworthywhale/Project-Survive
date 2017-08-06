﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttack : Ability
{

    private CMoveCombatable caster;

    string abilityName = "Attack";

    private int abilityDamage; //Scale to player damage?

    //The force to be applied to the caster in the attack direction
    private float abilityVelocity = 220;

    //Animation name in animator
    private string animation = "attack";

    //Knockback applied to target that is hit by attack
    private int abilityKnockback = 500; 
    private float cooldownTime = 0f;

    //Sound Variables
    private AudioClip abilityMissSound;
    private AudioClip abilityHitSound;

    //How far the ray will be cast
    private float abilityRange = 0.5f;

    //Time animation takes to get to the frame where it deals damage
    private float timeBeforeRay = 0.25f; 

    //Directional Variables
    private Vector2 pos;
    private Vector2 direction;


    public void setTarget(CMoveCombatable caster, Vector2 pos, Vector2 direction)
    {
        this.caster = caster;
        this.pos = pos;
        this.direction = direction;

        abilityDamage = caster.attackDamage;
    }

    public void setCooldown(bool cooldown)
    {
        return;
    }

    public bool onCooldown()
    {
        return false;
    }

    public float getCooldown()
    {
        return cooldownTime;
    }

    public float getAbilityVelocity()
    {
        return abilityVelocity;
    }

    public string getAnimation()
    {
        return animation;
    }

    public string getName()
    {
        return abilityName;
    }

    public IEnumerator getAction()
    {
        return abilityActionSequence(pos, direction);
    }

    public IEnumerator abilityActionSequence(Vector2 pos, Vector2 direction)
    {
        caster.rb2D.AddForce(direction * abilityVelocity);

        yield return new WaitForSeconds(timeBeforeRay);

        //Check if attack can go through
        if (!caster.isDead())
        {

            Vector2 newPos = new Vector2(caster.transform.position.x, caster.transform.position.y + caster.objectHeight / 2);

            RaycastHit2D[] hitObject = Physics2D.RaycastAll(newPos, direction, abilityRange, CMoveCombatable.attackMask, -10, 10);
            Debug.DrawRay(newPos, direction * abilityRange, Color.black, 3f);

            bool hitTarget = false;

            //If the Raycast hits an object on the layer Enemy
            foreach (RaycastHit2D r in hitObject)
            {
                if (r && r.transform.gameObject != caster.gameObject && caster.attacking)
                {
                    //Hit attack
                    CHitable objectHit = r.transform.gameObject.GetComponentInParent<CHitable>();

                    if (objectHit.isInvuln() || objectHit.tag == caster.tag || objectHit.isKnockedback())
                        continue;

                    //Apply damage and knockback
                    objectHit.setAttacker(caster);
                    objectHit.knockback(pos, abilityKnockback, objectHit.objectHeight); //Need to use original pos for knockback so the position of where you attacked from is the knockback
                    objectHit.loseHealth(abilityDamage);

                    caster.audioSource.clip = caster.attackSound;
                    caster.audioSource.Play();
                    
                    hitTarget = true;
                    break;
                }
            }

            if (!hitTarget)
            {
                caster.audioSource.clip = caster.missSound;
                caster.audioSource.Play();
            }

            yield return new WaitForSeconds(caster.pauseAfterAttack);
        }
        caster.canMove = true;
        caster.attacking = false;
    }

}