using System.Collections;
using System.Collections.Generic;
using ambiens.archtoolkit.atexplore.XNode;
using UnityEngine;

namespace ambiens.archtoolkit.atexplore.actionSystem
{
    
    public abstract class ATransformAnimation : ALoopableAction
    {

        public float animationDuration = 1;
        public float animationAmount = 90;

        protected bool isAnimating = false;

        protected float unitsPerSecond;
        protected float unitsApplied = 0;

        protected Vector3 PivotPosition;
        protected Vector3 PivotDirection;

        protected override void _RuntimeInit()
        {
            if (!this.InitSceneReferences())
            {
                return;
            }

            var g=ReferenceHolder.GetGameObject("pivot").GetComponent<ArchToolkit.AnimationSystem.GizmoRotateAroundPivot>();
            PivotPosition = g.position;
            PivotDirection = g.direction;
        }

        protected override bool _StartAction()
        {
            if (!isAnimating)
            {
                this.unitsApplied = 0;
                this.isAnimating = true;
                this.unitsPerSecond = this.currentSign * this.animationAmount / this.animationDuration;
                this.calculateFinalPosition();

                if (this.loopType == LoopType.PingPong)
                    this.currentSign = this.currentSign * -1;
            }

            return false;
        }

        public override void ManagedUpdate(float deltaTime)
        {
            this.CheckAnimation();
        }

        public abstract void animateFunction(float amount);
        public abstract void calculateAnimationAmount(float amount);

        private void CheckAnimation()
        {
            if (isAnimating)
            {

                float nextAnimation = this.unitsPerSecond * Time.deltaTime;

                this.unitsApplied += this.currentSign * nextAnimation;

                this.animateFunction(nextAnimation);

                if (Mathf.Abs(this.unitsApplied) >= Mathf.Abs(this.animationAmount))
                {
                    this.applyFinalPosition();
                    isAnimating = false;
                    if (this.AutoLoop)
                    {
                        this.StartAction();
                        this.StartNext();//AUTO LOOP MUST CALL THE NEXT ANIMATIONS!
                    }
                    else
                    {
                        this.OnComplete();
                    }
                }
            }
        }

        protected List<Quaternion> rotations = new List<Quaternion>();
        protected List<Vector3> positions = new List<Vector3>();

        public void calculateFinalPosition()
        {
            this.rotations.Clear();
            this.positions.Clear();

            this.animateFunction(this.animationAmount * currentSign);
            foreach (var t in this.SceneReferences)
            {
                this.rotations.Add(t.transform.rotation);
                this.positions.Add(t.transform.position);
            }
            this.animateFunction(-this.animationAmount * currentSign);
        }

        public void applyFinalPosition()
        {
            for (var i = 0; i < this.SceneReferences.Count; i++)
            {
                var t = this.SceneReferences[i].transform;
                t.rotation = this.rotations[i];
                t.position = this.positions[i];
            }
        }

    }

}
