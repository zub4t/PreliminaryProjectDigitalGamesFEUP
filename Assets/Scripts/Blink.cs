using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

public class Blink : MonoBehaviour
{
    public int Uses;
    public float cooldown, distance, speed, destinationMutiplier, cameraHeight;
    public Text UIText;
    public Transform cam;
    public LayerMask layermask;
    public StarterAssetsInputs _input;
    int maxUses;
    float cooldownTimer;
    bool blinking = false;
    Vector3 destination;
    ParticleSystem trail;
    // Start is called before the first frame update
    void Start()
    {
        trail = transform.Find("Trail").GetComponent<ParticleSystem>();
        maxUses = Uses;
        cooldownTimer = cooldown;
        UIText.text = Uses.ToString();


    }

    // Update is called once per frame
    void Update()
    {
        if (_input.blink)
        {
            PerformBlink();
            _input.blink = false;

        }
        if (Uses < maxUses)
        {
            if (cooldownTimer > 0)
            {
                cooldownTimer -= Time.deltaTime;

            }
            else
            {
                Uses += 1;
                cooldownTimer = cooldown;
                UIText.text = Uses.ToString();
            }
        }

        if (blinking)
        {
            var distance = Vector3.Distance(transform.position, destination);
            if (distance > 0.5f)
            {
                // move the player
                //transform.GetComponent<CharacterController>().Move(destination * speed  * Time.deltaTime);
                transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
            }
            else
            {
                blinking = false;

            }
        }

    }

    private void PerformBlink()
    {
        if (Uses > 0)
        {
            Uses -= 1;
            UIText.text = Uses.ToString();
            trail.Play();

            RaycastHit hit;
            if (Physics.Raycast(cam.position, cam.forward, out hit, distance, layermask))
            {
                destination = hit.point * destinationMutiplier;
                Debug.DrawLine(cam.position, hit.point * destinationMutiplier, Color.red, 2);

            }
            else
            {
                destination = (cam.position + cam.forward.normalized * distance) * destinationMutiplier;
                Debug.DrawLine(cam.position, (cam.position + cam.forward.normalized * distance), Color.green, 2);

            }
            destination.y += cameraHeight;
            blinking = true;
        }
    }
}
