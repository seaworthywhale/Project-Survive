﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicComboA : Ability
{

    private CMoveCombatable caster;

    string abilityName = "Basic Combo Finisher";

    //Ability Variables
    private int abilityDamage; //Scale to player damage?
    private float abilityVelocity = 5;
    private string animation = "attack";
    private int abilityKnockback = 500;
    private int abilityKnockUp = 500;
    private float cooldownTime = 0f;

    //Raycast Variables
    private float abilityRange = 0.4f;
    private float timeBeforeRay = 0.25f;

    //Directional Variables
    private Vector2 pos;
    private Vector2 direction;


    public void setTarget(CMoveCombatable caster, Vector2 pos, Vector2 direction)
    {
        this.caster = caster;
        this.pos = pos;
        this.direction = direction;

        abilityDamage = (int) (caster.attackDamage * 1.2f);
    }

    public Ability getComboAttack()
    {
        return this;
    }

    public void setCooldown(bool cooldown)
    {
        return; //No cooldown
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
        caster.rb2D.AddForce(direction * abilityVelocity / Time.timeScale);

        yield return new WaitForSeconds(timeBeforeRay);

        //Check if attack can go through
        if (!caster.isDead())
        {

            Vector2 newPos = new Vector2(caster.transform.position.x, caster.transform.position.y + caster.objectHeight / 2);

            RaycastHit2D[] hitObject = Physics2D.RaycastAll(newPos, direction, abilityRange, CMoveCombatable.attackMask, -10, 10);
            Debug.DrawRay(newPos, direction * abilityRange, Color.red, 3f);

            bool hitTarget = false;

            //If the Raycast hits an object on the layer Enemy
            foreach (RaycastHit2D r in hitObject)
            {

                if (r && r.transform.gameObject != caster.gameObject)
                {
                    //If an object has been hit first
                    if (r.transform.gameObject.tag == "Object")
                    {
                        if (r.collider.isTrigger)
                            continue;
                        else
                            break;
                    }

                    //Hit attack
                    CHitable objectHit = r.transform.gameObject.GetComponentInParent<CHitable>();

                    if (objectHit.tag == caster.tag)
                        continue;

                    //Apply damage and knockback
                    objectHit.setAttacker(caster);
                    objectHit.knockback(pos, abilityKnockback, objectHit.objectHeight); //Need to use original pos for knockback so the position of where you attacked from is the knockback
                    objectHit.knockUp(pos, abilityKnockback, abilityKnockUp, objectHit.objectHeight);
                    objectHit.loseHealth(abilityDamage);

                    caster.audioSource.clip = caster.attackSound;
                    caster.audioSource.Play();
                    caster.attackHit();

                    hitTarget = true;
                    break;
                }
            }

            if (!hitTarget)
            {
                caster.audioSource.clip = caster.missSound;
                caster.audioSource.Play();
            }

            if(caster.pauseAfterAttack < 0.5f)
                yield return new WaitForSeconds(0.5f);
            else
                yield return new WaitForSeconds(caster.pauseAfterAttack);
        }
        caster.canMove = true;
        caster.attacking = false;
    }

}