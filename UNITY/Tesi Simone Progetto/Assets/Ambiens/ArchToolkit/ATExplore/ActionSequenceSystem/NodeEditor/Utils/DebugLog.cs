using System.Collections;
using System.Collections.Generic;
using ambiens.archtoolkit.atexplore.XNode;
using UnityEngine;

namespace ambiens.archtoolkit.atexplore.actionSystem
{
    [CreateNodeMenuAttribute("Utils/Debug Log")]

    public class DebugLog : AAction
    {
        public string DebugString = "Debug";
        protected override void _RuntimeInit()
        {
            //DO Nothing
        }

        protected override bool _StartAction()
        {
            Debug.Log(DebugString);
            return true;
        }
    }
}
