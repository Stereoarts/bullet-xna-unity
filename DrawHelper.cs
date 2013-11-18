﻿/*
 * C# / XNA  port of Bullet (c) 2011 Mark Neale <xexuxjy@hotmail.com>
 *
 * Bullet Continuous Collision Detection and Physics Library
 * Copyright (c) 2003-2008 Erwin Coumans  http://www.bulletphysics.com/
 *
 * This software is provided 'as-is', without any express or implied warranty.
 * In no event will the authors be held liable for any damages arising from
 * the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose, 
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using System;
using System.Collections.Generic;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;

#if UNITY
using UnityEngine;
#else
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
#endif

namespace BulletXNA
{
#if UNITY
    public class VertexPositionColor
    {
        public VertexPositionColor()
        {
        }

        public VertexPositionColor(Vector3 position, Color color)
        {
        }
    }
#endif

    public static class DrawHelper
	{
		public static void DebugDrawConstraint(TypedConstraint constraint, IDebugDraw debugDraw)
		{
			bool drawFrames = (debugDraw.GetDebugMode() & DebugDrawModes.DBG_DrawConstraints) != 0;
			bool drawLimits = (debugDraw.GetDebugMode() & DebugDrawModes.DBG_DrawConstraintLimits) != 0;
			float dbgDrawSize = constraint.GetDbgDrawSize();
			if (dbgDrawSize <= 0f)
			{
				return;
			}

			switch (constraint.GetConstraintType())
			{
				case TypedConstraintType.POINT2POINT_CONSTRAINT_TYPE:
					{
						Point2PointConstraint p2pC = constraint as Point2PointConstraint;
						IndexedMatrix tr = IndexedMatrix.Identity;
						IndexedVector3 pivot = p2pC.GetPivotInA();
						pivot = p2pC.GetRigidBodyA().GetCenterOfMassTransform()* pivot;
						tr._origin = pivot;
						debugDraw.DrawTransform(ref tr, dbgDrawSize);
						// that ideally should draw the same frame	
						pivot = p2pC.GetPivotInB();
						pivot = p2pC.GetRigidBodyB().GetCenterOfMassTransform() * pivot;
						tr._origin = pivot;
						if (drawFrames) debugDraw.DrawTransform(ref tr, dbgDrawSize);
					}
					break;
				case TypedConstraintType.HINGE_CONSTRAINT_TYPE:
					{
						HingeConstraint pHinge = constraint as HingeConstraint;
						IndexedMatrix tr = pHinge.GetRigidBodyA().GetCenterOfMassTransform() * pHinge.GetAFrame();
						if (drawFrames)
						{
							debugDraw.DrawTransform(ref tr, dbgDrawSize);
						}
						tr = pHinge.GetRigidBodyB().GetCenterOfMassTransform() *  pHinge.GetBFrame();
						if (drawFrames)
						{
							debugDraw.DrawTransform(ref tr, dbgDrawSize);
						}
						float minAng = pHinge.GetLowerLimit();
						float maxAng = pHinge.GetUpperLimit();
						if (minAng == maxAng)
						{
							break;
						}
						bool drawSect = true;
						if (minAng > maxAng)
						{
							minAng = 0f;
							maxAng = MathUtil.SIMD_2_PI;
							drawSect = false;
						}
						if (drawLimits)
						{
							IndexedVector3 center = tr._origin;
							IndexedVector3 normal = tr._basis.GetColumn(2);
                            IndexedVector3 axis = tr._basis.GetColumn(0);
							IndexedVector3 zero = IndexedVector3.Zero;
							debugDraw.DrawArc(ref center, ref normal, ref axis, dbgDrawSize, dbgDrawSize, minAng, maxAng, ref zero, drawSect);
						}
					}
					break;
				case TypedConstraintType.CONETWIST_CONSTRAINT_TYPE:
					{
						ConeTwistConstraint pCT = constraint as ConeTwistConstraint;
						IndexedMatrix tr = pCT.GetRigidBodyA().GetCenterOfMassTransform() *  pCT.GetAFrame();
						if (drawFrames) debugDraw.DrawTransform(ref tr, dbgDrawSize);
						tr = pCT.GetRigidBodyB().GetCenterOfMassTransform() *  pCT.GetBFrame();
						if (drawFrames) debugDraw.DrawTransform(ref tr, dbgDrawSize);
						IndexedVector3 zero = IndexedVector3.Zero;

						if (drawLimits)
						{
							//const float length = float(5);
							float length = dbgDrawSize;
							const int nSegments = 8 * 4;
							float fAngleInRadians = MathUtil.SIMD_2_PI * (float)(nSegments - 1) / (float)nSegments;
							IndexedVector3 pPrev = pCT.GetPointForAngle(fAngleInRadians, length);
                            pPrev = tr * pPrev;
							for (int i = 0; i < nSegments; i++)
							{
								fAngleInRadians = MathUtil.SIMD_2_PI * (float)i / (float)nSegments;
								IndexedVector3 pCur = pCT.GetPointForAngle(fAngleInRadians, length);
                                pCur = tr * pCur;
								debugDraw.DrawLine(ref pPrev, ref pCur, ref zero);

								if (i % (nSegments / 8) == 0)
								{
									IndexedVector3 origin = tr._origin;
									debugDraw.DrawLine(ref origin, ref pCur, ref zero);
								}

								pPrev = pCur;
							}
							float tws = pCT.GetTwistSpan();
							float twa = pCT.GetTwistAngle();
							bool useFrameB = (pCT.GetRigidBodyB().GetInvMass() > 0f);
							if (useFrameB)
							{
								tr = pCT.GetRigidBodyB().GetCenterOfMassTransform() *  pCT.GetBFrame();
							}
							else
							{
								tr = pCT.GetRigidBodyA().GetCenterOfMassTransform() *  pCT.GetAFrame();
							}
							IndexedVector3 pivot = tr._origin;
                            IndexedVector3 normal = tr._basis.GetColumn(0);
                            IndexedVector3 axis = tr._basis.GetColumn(1);

							debugDraw.DrawArc(ref pivot, ref normal, ref axis, dbgDrawSize, dbgDrawSize, -twa - tws, -twa + tws, ref zero, true);
						}
					}
					break;
				case TypedConstraintType.D6_CONSTRAINT_TYPE:
				case TypedConstraintType.D6_SPRING_CONSTRAINT_TYPE:
					{
						Generic6DofConstraint p6DOF = constraint as Generic6DofConstraint;
						IndexedMatrix tr = p6DOF.GetCalculatedTransformA();
						if (drawFrames)
						{
							debugDraw.DrawTransform(ref tr, dbgDrawSize);
						}
						tr = p6DOF.GetCalculatedTransformB();
						if (drawFrames)
						{
							debugDraw.DrawTransform(ref tr, dbgDrawSize);
						}
						IndexedVector3 zero = IndexedVector3.Zero;
						if (drawLimits)
						{
							tr = p6DOF.GetCalculatedTransformA();
							IndexedVector3 center = p6DOF.GetCalculatedTransformB()._origin;
							// up is axis 1 not 2 ?

							IndexedVector3 up = tr._basis.GetColumn(1);
							IndexedVector3 axis = tr._basis.GetColumn(0);
							float minTh = p6DOF.GetRotationalLimitMotor(1).m_loLimit;
							float maxTh = p6DOF.GetRotationalLimitMotor(1).m_hiLimit;
							float minPs = p6DOF.GetRotationalLimitMotor(2).m_loLimit;
							float maxPs = p6DOF.GetRotationalLimitMotor(2).m_hiLimit;
							debugDraw.DrawSpherePatch(ref center, ref up, ref axis, dbgDrawSize * .9f, minTh, maxTh, minPs, maxPs, ref zero);
                            axis = tr._basis.GetColumn(1);
							float ay = p6DOF.GetAngle(1);
							float az = p6DOF.GetAngle(2);
							float cy = (float)Math.Cos(ay);
							float sy = (float)Math.Sin(ay);
							float cz = (float)Math.Cos(az);
							float sz = (float)Math.Sin(az);
							IndexedVector3 ref1 = new IndexedVector3(
							    cy * cz * axis.X + cy * sz * axis.Y - sy * axis.Z,
							    -sz * axis.X + cz * axis.Y,
							    cz * sy * axis.X + sz * sy * axis.Y + cy * axis.Z);
							tr = p6DOF.GetCalculatedTransformB();
                            IndexedVector3 normal = -tr._basis.GetColumn(0);
							float minFi = p6DOF.GetRotationalLimitMotor(0).m_loLimit;
							float maxFi = p6DOF.GetRotationalLimitMotor(0).m_hiLimit;
							if (minFi > maxFi)
							{
								debugDraw.DrawArc(ref center, ref normal, ref ref1, dbgDrawSize, dbgDrawSize, -MathUtil.SIMD_PI, MathUtil.SIMD_PI, ref zero, false);
							}
							else if (minFi < maxFi)
							{
								debugDraw.DrawArc(ref center, ref normal, ref ref1, dbgDrawSize, dbgDrawSize, minFi, maxFi, ref zero, false);
							}
							tr = p6DOF.GetCalculatedTransformA();
							IndexedVector3 bbMin = p6DOF.GetTranslationalLimitMotor().m_lowerLimit;
							IndexedVector3 bbMax = p6DOF.GetTranslationalLimitMotor().m_upperLimit;
							debugDraw.DrawBox(ref bbMin, ref bbMax, ref tr, ref zero);
						}
					}
					break;
				case TypedConstraintType.SLIDER_CONSTRAINT_TYPE:
					{
						SliderConstraint pSlider = constraint as SliderConstraint;
						IndexedMatrix tr = pSlider.GetCalculatedTransformA();
						if (drawFrames) debugDraw.DrawTransform(ref tr, dbgDrawSize);
						tr = pSlider.GetCalculatedTransformB();
						if (drawFrames) debugDraw.DrawTransform(ref tr, dbgDrawSize);
						IndexedVector3 zero = IndexedVector3.Zero;
						if (drawLimits)
						{
							IndexedMatrix tr2 = pSlider.GetCalculatedTransformA();
							IndexedVector3 li_min = tr2 * new IndexedVector3(pSlider.GetLowerLinLimit(), 0f, 0f);
							IndexedVector3 li_max = tr2 * new IndexedVector3(pSlider.GetUpperLinLimit(), 0f, 0f);
							debugDraw.DrawLine(ref li_min, ref li_max, ref zero);
                            IndexedVector3 normal = tr._basis.GetColumn(0);
                            IndexedVector3 axis = tr._basis.GetColumn(1);
							float a_min = pSlider.GetLowerAngLimit();
							float a_max = pSlider.GetUpperAngLimit();
							IndexedVector3 center = pSlider.GetCalculatedTransformB()._origin;
							debugDraw.DrawArc(ref center, ref normal, ref axis, dbgDrawSize, dbgDrawSize, a_min, a_max, ref zero, true);
						}
					}
					break;
				default:
					break;
			}
			return;
		}

		public static ShapeData CreateCube()
		{
			IndexedMatrix identity = IndexedMatrix.Identity;
#if UNITY
			return CreateBox(IndexedVector3.Zero, new IndexedVector3(1), Color.yellow, ref identity);
#else
			return CreateBox(IndexedVector3.Zero, new IndexedVector3(1), Color.Yellow, ref identity);
#endif
		}

		public static ShapeData CreateBox(IndexedVector3 position, IndexedVector3 sideLength, Color color, ref IndexedMatrix transform)
		{
			ShapeData shapeData = new ShapeData(8, 36);
			int index = 0;
			shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(0, 0, 0))).ToVector3(), color);
            shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(sideLength.X, 0, 0))).ToVector3(), color);
            shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(sideLength.X, 0, sideLength.Z))).ToVector3(), color);
            shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(0, 0, sideLength.Z))).ToVector3(), color);
            shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(0, sideLength.Y, 0))).ToVector3(), color);
            shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(sideLength.X, sideLength.Y, 0))).ToVector3(), color);
            shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(sideLength.X, sideLength.Y, sideLength.Z))).ToVector3(), color);
            shapeData.m_verticesArray[index++] = new VertexPositionColor((transform * (position + new IndexedVector3(0, sideLength.Y, sideLength.Z))).ToVector3(), color);
			shapeData.m_indexArray = DrawHelper.s_cubeIndices;
			return shapeData;
		}

		public static ShapeData CreateSphere(int slices, int stacks, float radius, Color color)
		{
			ShapeData shapeData = new ShapeData((slices + 1) * (stacks + 1), (slices * stacks * 6));

			float phi = 0f;
			float theta = 0f; ;
#if UNITY
			float deltaPhi = Mathf.PI / stacks;
			float dtheta = Mathf.PI * 2.0f / slices;
#else
			float deltaPhi = MathHelper.Pi / stacks;
			float dtheta = MathHelper.TwoPi / slices;
#endif
			float x, y, z, sc;

			short index = 0;

			for (int stack = 0; stack <= stacks; stack++)
			{
#if UNITY
				phi = Mathf.PI / 2.0f - (stack * deltaPhi);
#else
				phi = MathHelper.PiOver2 - (stack * deltaPhi);
#endif
				y = radius * (float)Math.Sin(phi);
				sc = -radius * (float)Math.Cos(phi);

				for (int slice = 0; slice <= slices; slice++)
				{
					theta = slice * dtheta;
					x = sc * (float)Math.Sin(theta);
					z = sc * (float)Math.Cos(theta);

					//s_sphereVertices[index++] = new VertexPositionNormalTexture(new IndexedVector3(x, y, z),
					//                            new IndexedVector3(x, y, z),
					//                            new Vector2((float)slice / (float)slices, (float)stack / (float)stacks));
					shapeData.m_verticesArray[index++] = new VertexPositionColor(new Vector3(x, y, z), color);
				}
			}
			int stride = slices + 1;
			index = 0;
			for (int stack = 0; stack < stacks; stack++)
			{
				for (int slice = 0; slice < slices; slice++)
				{
					shapeData.m_indexList[index++] = (short)((stack + 0) * stride + slice);
					shapeData.m_indexList[index++] = (short)((stack + 1) * stride + slice);
					shapeData.m_indexList[index++] = (short)((stack + 0) * stride + slice + 1);

					shapeData.m_indexList[index++] = (short)((stack + 0) * stride + slice + 1);
					shapeData.m_indexList[index++] = (short)((stack + 1) * stride + slice);
					shapeData.m_indexList[index++] = (short)((stack + 1) * stride + slice + 1);
				}
			}
			return shapeData;
		}


		public static short[] s_cubeIndices = new short[]{
                             0,1,2,2,3,0, // face A
                             0,1,5,5,4,0, // face B
                             1,2,6,6,5,1, // face c
                             2,6,7,7,3,2, // face d
                             3,7,4,4,0,3, // face e
                             4,5,6,6,7,4}; // face f
	}

	public class DebugDrawcallback : ITriangleCallback, IInternalTriangleIndexCallback,IDisposable
	{
		IDebugDraw m_debugDrawer;
		IndexedVector3 m_color;
		IndexedMatrix m_worldTrans;

		public virtual bool graphics()
		{
			return true;
		}

        public DebugDrawcallback() { } // for pool

		public DebugDrawcallback(IDebugDraw debugDrawer, ref IndexedMatrix worldTrans, ref IndexedVector3 color)
		{
			m_debugDrawer = debugDrawer;
			m_color = color;
			m_worldTrans = worldTrans;
		}

        public void Initialise(IDebugDraw debugDrawer, ref IndexedMatrix worldTrans, ref IndexedVector3 color)
        {
            m_debugDrawer = debugDrawer;
            m_color = color;
            m_worldTrans = worldTrans;
        }


		public virtual void InternalProcessTriangleIndex(IndexedVector3[] triangle, int partId, int triangleIndex)
		{
			ProcessTriangle(triangle, partId, triangleIndex);
		}

		public virtual void ProcessTriangle(IndexedVector3[] triangle, int partId, int triangleIndex)
		{
			//(void)partId;
			//(void)triangleIndex;

			IndexedVector3 wv0, wv1, wv2;
            wv0 = m_worldTrans * triangle[0];
            wv1 = m_worldTrans * triangle[1];
            wv2 = m_worldTrans * triangle[2];

            if ((int)(m_debugDrawer.GetDebugMode() & DebugDrawModes.DBG_DrawNormals) != 0)
            {
                IndexedVector3 center = (wv0 + wv1 + wv2) * (1f / 3f);
                IndexedVector3 normal = (wv1 - wv0).Cross(wv2 - wv0);
                normal.Normalize();
                IndexedVector3 normalColor = new IndexedVector3(1, 1, 0);
                //m_debugDrawer.DrawLine(center, center + normal, normalColor);
            }

			m_debugDrawer.DrawLine(ref wv0, ref wv1, ref m_color);
			m_debugDrawer.DrawLine(ref wv1, ref wv2, ref m_color);
			m_debugDrawer.DrawLine(ref wv2, ref wv0, ref m_color);
		}

		//public static void drawUnitSphere(GraphicsDevice gd)
		//{
		//    gd.VertexDeclaration = s_vertexDeclaration;
		//    int primCount = s_sphereIndices.Length / 3;
		//    //int primCount = 2;
		//    int indexStart = 0;
		//    gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, s_sphereVertices, 0, s_sphereVertices.Length, s_sphereIndices, indexStart, primCount);
		//}
		public virtual void Cleanup()
		{
		}

        public void Dispose()
        {
            BulletGlobals.DebugDrawcallbackPool.Free(this);
        }

	}
	
	public class ShapeData
	{
		public ShapeData()
		{
			m_verticesList = new List<VertexPositionColor>();
			m_indexList = new List<short>();
		}

		public ShapeData(int numVert, int numIndices)
		{
			m_verticesArray = new VertexPositionColor[numVert];
			m_indexArray = new short[numIndices];
		}

		public VertexPositionColor[] m_verticesArray;
		public IList<VertexPositionColor> m_verticesList;
		public short[] m_indexArray; // This will remain fixed regardless so try and re-use
		public IList<short> m_indexList; // or if not appropiate then this one.
	}
}
