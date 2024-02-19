using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Mathematics.LinearAlgebra;

using PositionBasedDynamics.Bodies;

namespace PositionBasedDynamics.Collisions
{
    public abstract class CollisionExternal3d
    {
        internal virtual void FindExternalContacts(IList<Body3d> bodies, IList<Rigidbody> externalBodies, List<CollisionContactExternal3d> externalContacts)
        {

        }
    }
}