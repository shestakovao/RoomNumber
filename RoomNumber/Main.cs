using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoomNumber
{
    [Transaction(TransactionMode.Manual)]
    public class Main : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;
            

            List<Room> rooms = new FilteredElementCollector(doc, doc.ActiveView.Id)
                               .OfCategory(BuiltInCategory.OST_Rooms)
                               .OfType<Room>()
                               .ToList();


            if (rooms.Count == 0)
            {
                TaskDialog.Show("Ошибка", "Не найдено помещений на текущем виде");
                return Result.Cancelled;
            }

            RoomTagType NewType = new FilteredElementCollector(doc)
                               .OfCategory(BuiltInCategory.OST_RoomTags)
                               .OfType<RoomTagType>()
                               .Cast<RoomTagType>()
                               .Where(x => x.Name.Equals("Номер помещения"))
                               .FirstOrDefault();

            {
                Transaction transaction = new Transaction(doc);
                transaction.Start("Вставка номеров помещений");
                foreach (Room room in rooms)
                {
                    XYZ roomCenter = GetRoomCenter(room);
                    UV center = new UV(roomCenter.X, roomCenter.Y);
                    RoomTag roomTag = doc.Create.NewRoomTag(new LinkElementId(room.Id), center, doc.ActiveView.Id);
                    roomTag.ChangeTypeId(NewType.Id);
                 }
                transaction.Commit();
            }

            return Result.Succeeded;
        }
        public XYZ GetRoomCenter(Room room)
        {
            // Get the room center point.
            XYZ boundCenter = GetElementCenter(room);
            LocationPoint locPt = (LocationPoint)room.Location;
            XYZ roomCenter = new XYZ(boundCenter.X, boundCenter.Y, locPt.Point.Z);
            return roomCenter;
        }
        public XYZ GetElementCenter(Element element)
        {
            BoundingBoxXYZ bounding = element.get_BoundingBox(null);
            return (bounding.Max + bounding.Min) / 2;
        }
    }


}
