using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.EditorInput;

namespace KeepAttributesHorizontal
{
    public class MyCommands
    {
        // Custom Overrule Class
        public class KeepStraightOverrule : TransformOverrule
        {
            public override void TransformBy(Entity entity, Matrix3d transform)
            {
                // Apply normal transformation first
                base.TransformBy(entity, transform);

                // Apply horizontal constraint depending on entity type

                if (entity is AttributeReference attRef)
                {
                    attRef.Rotation = 0.0;
                }
                else if (entity is DBText dbText)
                {
                    dbText.Rotation = 0.0;
                }
                else if (entity is MText mText)
                {
                    mText.Rotation = 0.0;
                }
            }
        }

        // Store Overrule Instance
        static KeepStraightOverrule? myOverRule;


        // ENABLE COMMAND

        [CommandMethod("KeepStraight")]
        public static void EnableKeepStraight()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            // Create overrule only once
            if (myOverRule == null)
            {
                myOverRule = new KeepStraightOverrule();

                // Apply to AttributeReference
                TransformOverrule.AddOverrule(
                    RXClass.GetClass(typeof(AttributeReference)),
                    myOverRule,
                    false
                );

                // Apply to DBText
                TransformOverrule.AddOverrule(
                    RXClass.GetClass(typeof(DBText)),
                    myOverRule,
                    false
                );

                // Apply to MText
                TransformOverrule.AddOverrule(
                    RXClass.GetClass(typeof(MText)),
                    myOverRule,
                    false
                );
            }

            // Enable overruling globally
            TransformOverrule.Overruling = true;

            ed.WriteMessage("\nKeepStraight ENABLED: Text will stay horizontal.\n");
        }


        //  DISABLE COMMAND keepstraight

        [CommandMethod("KeepStraightOff")]
        public static void DisableKeepStraight()
        {
            Editor ed = AcadApp.DocumentManager.MdiActiveDocument.Editor;

            if (myOverRule != null)
            {
                // Remove overrule from all entity types
                TransformOverrule.RemoveOverrule(
                    RXClass.GetClass(typeof(AttributeReference)),
                    myOverRule
                );

                TransformOverrule.RemoveOverrule(
                    RXClass.GetClass(typeof(DBText)),
                    myOverRule
                );

                TransformOverrule.RemoveOverrule(
                    RXClass.GetClass(typeof(MText)),
                    myOverRule
                );

                myOverRule = null;
            }

            ed.WriteMessage("\nKeepStraight DISABLED: Text will rotate normally.\n");
        }
    }
}