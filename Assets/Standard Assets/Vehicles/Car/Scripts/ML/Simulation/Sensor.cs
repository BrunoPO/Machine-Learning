using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    public class Sensor : MonoBehaviour
    {

        #region Members
        // The layer this sensor will be reacting to, to be set in Unity editor.
        [SerializeField]
        private LayerMask LayerToSense;
        //The crosshair of the sensor, to be set in Unity editor.
        [SerializeField]
        private SpriteRenderer Cross;

        // Max and min readings
        private const float MAX_DIST = 80f;
        private const float MIN_DIST = 0.01f;

        /// <summary>
        /// The current sensor readings in percent of maximum distance.
        /// </summary>
        public float Output
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        void Start()
        {
            //Cross.gameObject.SetActive(true);
        }
        #endregion

        #region Methods
        // Unity method for updating the simulation
        void FixedUpdate()
        {
            //Calculate direction of sensor
            Vector3 heading = this.transform.position - this.transform.parent.transform.position;
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;

            //Send raycast into direction of sensor
            RaycastHit hit;
            Physics.Raycast(this.transform.parent.transform.position, direction, out hit, MAX_DIST, LayerToSense, QueryTriggerInteraction.UseGlobal);
            Debug.DrawRay(this.transform.parent.transform.position, direction * MAX_DIST, Color.yellow);
            //Check distance
            if (hit.collider == null)
                hit.distance = MAX_DIST;
            else if (hit.distance < MIN_DIST)
                hit.distance = MIN_DIST;

            this.Output = hit.distance; //transform to percent of max distance
            //Cross.transform.position = (Vector2)this.transform.position + direction * hit.distance; //Set position of visual cross to current reading
        }
        private void OnDrawGizmos()
        {
            Vector3 heading = this.transform.position - this.transform.parent.transform.position;
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            //direction.Normalize();
            Debug.DrawRay(this.transform.parent.transform.position, direction * MAX_DIST, Color.yellow);
        }

        /// <summary>
        /// Hides the crosshair of this sensor.
        /// </summary>
        public void Hide()
        {
            //Cross.gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the crosshair of this sensor.
        /// </summary>
        public void Show()
        {
            //Cross.gameObject.SetActive(true);
        }
        #endregion

    }
}