using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DuskOfTheUniverse
{
    class FileManager
    {
        public static void SaveUsers(StoredDataContainer data, string filename)
        {
            string path = "H:\\CollegeStuff\\Graded Unit 2\\";

            // Get the path of the save game
            string fullpath = Path.Combine(path, filename);

            XmlWriterSettings settings = new XmlWriterSettings();
            XmlSerializer serializer = new XmlSerializer(typeof(StoredDataContainer));

            try
            {
                using (StreamWriter streamWriter = new StreamWriter(fullpath))
                {
                     try
                     {
                         XmlWriter writer;
                         if (data == null)
                             writer = XmlWriter.Create(fullpath, settings);

                         serializer.Serialize(streamWriter, data);
                     }
                     catch(Exception e)
                     {
                     }
                     finally
                     {
                         streamWriter.Close();
                     }
                }
            }

            catch (Exception e)
            {
            }
        }

        public static StoredDataContainer LoadUsers(string filename)
        {
            string path = "H:\\CollegeStuff\\Graded Unit 2\\";

            // Get the path of the save game
            string fullpath = Path.Combine(path, filename);

            XmlSerializer serializer = new XmlSerializer(typeof(StoredDataContainer));

            try
            {
                StoredDataContainer data = new StoredDataContainer();

                using (StreamReader streamReader = new StreamReader(fullpath))
                {
                    try
                    {
                        data = (StoredDataContainer)serializer.Deserialize(streamReader);
                    }
                    catch (Exception e)
                    {
                    }
                    finally
                    {
                        streamReader.Close();
                    }
                }

                return data;

            }

            catch (Exception e)
            {
            }

            return null;
        }
    }
}
