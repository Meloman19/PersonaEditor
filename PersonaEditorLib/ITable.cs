using System.Xml.Linq;

namespace PersonaEditorLib
{
    public interface ITable
    {
        XDocument GetTable();
        void SetTable(XDocument xDocument);
    }
}