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

                // Decide conditionally whether to keep straight
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
            }

            // 🔹 CONDITION LOGIC (your "AI placeholder")
            private bool ShouldKeepStraight(Entity entity)
            {
                // Example rules — you can tweak these

                // Rule 1: Check layer name
                if (entity.Layer.ToUpper().Contains("TITLE") ||
                    entity.Layer.ToUpper().Contains("LABEL"))
                    return true;

                // Rule 2: Check text height (bigger = likely important)
                double height = GetTextHeight(entity);
                if (height > 5.0)   // adjust based on your drawing scale
                    return true;

                // Rule 3: Near-horizontal already → keep it horizontal
                double rot = GetRotation(entity);
                if (IsNearlyHorizontal(rot))
                    return true;

                // Otherwise → let AutoCAD handle rotation
                return false;
            }

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

            private bool IsNearlyHorizontal(double rotation)
            {
                double tol = 10 * (System.Math.PI / 180.0); // 10 degrees tolerance

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
            }

            TransformOverrule.Overruling = true;

            ed.WriteMessage("\nSmartKeepStraight ENABLED.\n");
        }

        // ============================================================
        // ❌ DISABLE
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

                myOverRule = null;
            }

            ed.WriteMessage("\nSmartKeepStraight DISABLED.\n");
        }
    }
}