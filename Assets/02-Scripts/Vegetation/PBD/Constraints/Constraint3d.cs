using System;
using System.Collections.Generic;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Constraints
{

    public abstract class Constraint3d
    {
        #region Instance Properties - Constraint Properties

        protected Body3d Body { get; private set; }

        #endregion

        #region Virtual Properties

        public virtual int[] ParticleConstrained { get; set; }
        public virtual Body3d BodyConstrained { get; set; }
        public virtual double CurrentStiffness { get; set; }

        #endregion

        internal Constraint3d(Body3d body)
        {
            Body = body;
        }

        #region Virtual Methods

        internal virtual void ConstrainPositions(double di)
        {

        }

        internal virtual void ConstrainVelocities()
        {

        }

        internal virtual void RemoveVerticalConstrainPositions()
        {

        }

        internal virtual void RecoverVerticalConstrainPositions(List<Constraint3d> BrokenConstraints)
        {

        }

        internal virtual void ModifyStepWiseDegradation()
        {
            
        }

        #endregion
    }
}