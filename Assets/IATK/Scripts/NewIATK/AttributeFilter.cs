using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace NewIATK
{
    [Serializable]
    public class AttributeFilter
    {
        [Tooltip("The name of this dimension.")] [SerializeField]
        public string Name = "Undefined";

        [Tooltip("Minimum filter value for the attribute.")] [Range(0f, 1f)] [SerializeField]
        public float MinFilter = 0f;

        [Tooltip("Maximum filter value for the attribute.")] [Range(0f, 1f)] [SerializeField]
        public float MaxFilter = 1f;

        [Tooltip("Minimum scaling value for the attribute.")] [Range(0f, 1f)] [SerializeField]
        public float MinScale = 0f;

        [Tooltip("Maximum scaling value for the attribute.")] [Range(0f, 1f)] [SerializeField]
        public float MaxScale = 1f;
    }
}
