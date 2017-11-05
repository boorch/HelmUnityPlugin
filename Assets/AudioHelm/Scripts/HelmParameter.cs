// Copyright 2017 Matt Tytel

using UnityEngine;
using UnityEngine.Audio;

namespace AudioHelm
{
    /// <summary>
    /// A single Helm synthesizer parameter to control.
    /// </summary>
    [System.Serializable]
    public class HelmParameter
    {
        public HelmParameter()
        {
            parent = null;
        }

        public HelmParameter(HelmController par)
        {
            parent = par;
        }

        public HelmParameter(HelmController par, Param param)
        {
            parent = par;
            parameter = param;
            paramValue_ = parent.GetParameterPercent(parameter);
        }

        /// <summary>
        /// The parameter index.
        /// </summary>
        public Param parameter = Param.kNone;

        /// <summary>
        /// The controller this parameter belongs to.
        /// </summary>
        public HelmController parent = null;

        float lastValue = -1.0f;

        [SerializeField]
        float paramValue_ = 0.0f;
        /// <summary>
        /// The current parameter value.
        /// </summary>
        public float paramValue
        {
            get
            {
                return paramValue_;
            }
            set
            {
                if (paramValue_ == value)
                    return;

                paramValue_ = value;
                UpdateNative();
            }
        }

        public void UpdateNative()
        {
            if (parent && parameter != Param.kNone && lastValue != paramValue_)
                parent.SetParameterPercent(parameter, paramValue_);
            lastValue = paramValue_;
        }
    }
}
