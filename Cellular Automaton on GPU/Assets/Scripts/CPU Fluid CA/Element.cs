using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CPUFluid
{
    /// <summary>
    /// Data structure representing the properties of elements
    /// </summary>
    public struct Element
    {
        int id;
        public int viscosity;
        public float density;
        //the temperature in celcius at whitch the element will change state to element with evaporateElementId
        public float evaporateTemperature;
        public int evaporateElementId;
        //the temperature in celcius at whitch the element will change state to element with freezeElementId
        public float freezeTemperature;
        public int freezeElementId;

        public Element(int id, int viscosity, float density)
        {
            this.id = id;
            this.viscosity = viscosity;
            this.density = density;
            evaporateTemperature = float.MaxValue;
            evaporateElementId = id;
            freezeTemperature = float.MinValue;
            freezeElementId = id;
        }

        /// <summary>
        /// Sets the freeze transition
        /// </summary>
        /// <param name="freezeTemp">the temperature in celcius at whitch the element will change state</param>
        /// <param name="freezeElementId">The element Id of the element that it will change state to</param>
        public void setFreezeTransition(float freezeTemp, int freezeElementId)
        {
            freezeTemperature = freezeTemp;
            this.freezeElementId = freezeElementId;
        }

        /// <summary>
        /// Sets the evaporate transition
        /// </summary>
        /// <param name="evaporateTemp">The temperature in celcius at whitch the element will change state</param>
        /// <param name="evaporateElementId">The element Id of the element that it will change state to</param>
        public void setEvaporateTransition(float evaporateTemp, int evaporateElementId)
        {
            evaporateTemperature = evaporateTemp;
            this.evaporateElementId = evaporateElementId;
        }
    }
}
