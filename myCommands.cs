using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.EditorInput;

namespace KeepAttributesHorizontal
{
    public class MyCommands
    {
        // 🔹 Custom Overrule Class
        public class KeepStraightOverrule : TransformOverrule
        {
            public override void TransformBy(Entity entity, Matrix3d transform)
            {
                // Apply normal transformation first
                base.TransformBy(entity, transform);

                // 🔹 TEXT HANDLING
                if (entity is DBText dbText)
                {
                    if (ShouldKeepStraight(dbText))
                        dbText.Rotation = 0.0;
                }
                else if (entity is MText mText)
                {
                    if (ShouldKeepStraight(mText))
                        mText.Rotation = 0.0;
                }
                else if (entity is AttributeReference attRef)
                {
                    if (ShouldKeepStraight(attRef))
                        attRef.Rotation = 0.0;
                }

                // 🔹 DIMENSION HANDLING (NEW)
                else if (entity is Dimension dim)
                {
                    if (ShouldKeepStraight(dim))
                        ForceDimensionHorizontal(dim);
                }
            }

            // ============================================================
            // 🔹 TEXT LOGIC
            // ============================================================
            private bool ShouldKeepStraight(Entity entity)
            {
                // Rule 1: Layer-based importance
                if (entity.Layer.ToUpper().Contains("TITLE") ||
                    entity.Layer.ToUpper().Contains("LABEL"))
                    return true;

                // Rule 2: Large text
                double height = GetTextHeight(entity);
                if (height > 400.0) // adjust based on your scale
                    return true;

                // Rule 3: Already near horizontal
                double rot = GetRotation(entity);
                if (IsNearlyHorizontal(rot))
                    return true;

                return false;
            }

            // ============================================================
            // 🔹 DIMENSION LOGIC
            // ============================================================
            private bool ShouldKeepStraight(Dimension dim)
            {
                // Rule 1: Important dimension layers
                if (dim.Layer.ToUpper().Contains("DIM") ||
                    dim.Layer.ToUpper().Contains("ANNOTATION"))
                    return true;

                // Rule 2: Large dimension text
                if (dim.Dimtxt > 200.0)
                    return true;

                // Rule 3: Dimension already near horizontal
                double angle = GetDimensionAngle(dim);
                if (IsNearlyHorizontal(angle))
                    return true;

                return false;
            }

            // ============================================================
            // 🔹 FORCE DIMENSION TEXT HORIZONTAL
            // ============================================================
            private void ForceDimensionHorizontal(Dimension dim)
            {
                try
                {
                    dim.UpgradeOpen();

                    // Force horizontal text
                    dim.TextRotation = 0.0;

                    // Improve readability
                    dim.Dimtix = false;

                    dim.DowngradeOpen();
                }
                catch
                {
                    // Prevent AutoCAD crash
                }
            }

            // ============================================================
            // 🔹 HELPERS
            // ============================================================
            private double GetTextHeight(Entity entity)
            {
                if (entity is DBText db) return db.Height;
                if (entity is MText mt) return mt.TextHeight;
                if (entity is AttributeReference at) return at.Height;

                return 0.0;
            }

            private double GetRotation(Entity entity)
            {
                if (entity is DBText db) return db.Rotation;
                if (entity is MText mt) return mt.Rotation;
                if (entity is AttributeReference at) return at.Rotation;

                return 0.0;
            }

            private double GetDimensionAngle(Dimension dim)
            {
                if (dim is RotatedDimension rd)
                    return rd.Rotation;

                if (dim is AlignedDimension ad)
                {
                    Vector3d dir = ad.XLine1Point.GetVectorTo(ad.XLine2Point);
                    return dir.AngleOnPlane(new Plane(Point3d.Origin, Vector3d.ZAxis));
                }

                return 0.0;
            }

            private bool IsNearlyHorizontal(double rotation)
            {
                double tol = 10 * (System.Math.PI / 180.0);

                return (System.Math.Abs(rotation) < tol ||
                        System.Math.Abs(rotation - System.Math.PI) < tol);
            }
        }

        // 🔹 Overrule instance
        static KeepStraightOverrule? myOverRule;

        // ============================================================
        // ✅ ENABLE
        // ============================================================
        [CommandMethod("SmartKeepStraight")]
        public static void EnableKeepStraight()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            if (myOverRule == null)
            {
                myOverRule = new KeepStraightOverrule();

                TransformOverrule.AddOverrule(
                    RXClass.GetClass(typeof(DBText)), myOverRule, false);

                TransformOverrule.AddOverrule(
                    RXClass.GetClass(typeof(MText)), myOverRule, false);

                TransformOverrule.AddOverrule(
                    RXClass.GetClass(typeof(AttributeReference)), myOverRule, false);

                TransformOverrule.AddOverrule(
                    RXClass.GetClass(typeof(Dimension)), myOverRule, false);
            }

            TransformOverrule.Overruling = true;

            ed.WriteMessage("\nSmartKeepStraight ENABLED.\n");
        }

        // ============================================================
        //  DISABLE
        // ============================================================
        [CommandMethod("SmartKeepStraightOff")]
        public static void DisableKeepStraight()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            if (myOverRule != null)
            {
                TransformOverrule.RemoveOverrule(
                    RXClass.GetClass(typeof(DBText)), myOverRule);

                TransformOverrule.RemoveOverrule(
                    RXClass.GetClass(typeof(MText)), myOverRule);

                TransformOverrule.RemoveOverrule(
                    RXClass.GetClass(typeof(AttributeReference)), myOverRule);

                TransformOverrule.RemoveOverrule(
                    RXClass.GetClass(typeof(Dimension)), myOverRule);

                myOverRule = null;
            }

            ed.WriteMessage("\nSmartKeepStraight DISABLED.\n");
        }
    }
}