using SteamLauncher.Attributes;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SteamLauncher.Logging;
using SteamLauncher.Tools;
using System.Runtime.Serialization;
using System.Text;

namespace SteamLauncher.DataStore
{
    //[Serializable]
    public abstract class XmlFile<T> where T : XmlFile<T>
    {
        /// <summary>
        /// Retrieves the filename of the class that implemented <see cref="XmlFilename"/> by reading its
        /// <see cref="XmlFilenameAttribute"/>.
        /// </summary>
        [XmlIgnore]
        public static string XmlFilename => (typeof(T).GetCustomAttributes(typeof(XmlFilenameAttribute), 
                                                                           true)[0] as XmlFilenameAttribute)?.Filename;

        /// <summary>
        /// Retrieves the file path of <see cref="XmlFilename"/> by combining it with
        /// <see cref="Info.SteamLauncherDir"/>.
        /// </summary>
        [XmlIgnore]
        public static string XmlPath => Path.Combine(Info.SteamLauncherDir, XmlFilename);

        /// <summary>
        /// Retrieves the URL at which a subclass of <see cref="XmlFilename"/> can be updated online (by reading its
        /// <see cref="XmlUrlAttribute"/>). Only special classes that can be updated online will set this attribute.
        /// </summary>
        [XmlIgnore]
        public static string XmlUrl => (typeof(T).GetCustomAttributes(typeof(XmlUrlAttribute), 
                                                                      true)[0] as XmlUrlAttribute)?.Url;


        ///// <summary>
        ///// Used as a means of allowing derived classes to run code after deserialization is finished.
        ///// </summary>
        //public virtual void RunAfterDeserialize() { }

        #region Save/Serialize File

        ///// <summary>
        ///// Used as a means of getting the access to the static 'XML_PATH' variables in derived classes within this
        ///// parent abstract class (which doesn't allow static variables). [Yes, I know this is terrible design and
        ///// needs to be changed... its on the list.]
        ///// </summary>
        //public abstract string SavePath { get; }

        /// <summary>
        /// Serializes T and saves it to a file.
        /// </summary>
        public void Save()
        {
            Serialize();

            //var ns = new XmlSerializerNamespaces();
            //ns.Add("", "");

            //var serializer = new XmlSerializer(typeof(T));

            //using (var fStream = new FileStream(path, FileMode.Create))
            //    serializer.Serialize(fStream, this, ns);

            Logger.Info($"'{typeof(T).Name}' data file saved.");
        }

        /// <summary>
        /// Serialize class instance to an XML string.
        /// </summary>
        /// <typeparam name="T">Class type.</typeparam>
        /// <returns>A string containing the serialized class instance.</returns>
        protected string SerializeToString()
        {
            var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            //ns.Add("", "");

            var xmlWriterSettings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = false,
                Indent = true
            };

            using (var ms = new MemoryStream())
            using (var writer = XmlWriter.Create(ms, xmlWriterSettings))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, this, ns);
                var serializedString = Encoding.UTF8.GetString(ms.ToArray());
                Logger.Info($"Successfully serialized {XmlFilename} to string.");
                return serializedString;
            }
        }

        /// <summary>
        /// Serialize class instance to a file.
        /// </summary>
        /// <typeparam name="T">Class type.</typeparam>
        /// <param name="path">The file path of the XML file to save the serialized data to.</param>
        protected void Serialize(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = XmlPath;

            var serializedStr = SerializeToString();
            File.WriteAllText(path, serializedStr);

            Logger.Info($"Successfully saved {XmlFilename} to file.");
        }

        #endregion

        #region Load/Deserialize Data

        //[OnDeserialized]
        //internal void OnDeserialized(StreamingContext context)
        //{
        //    OnDeserialized();
        //}

        /// <summary>
        /// If implemented in child class, this function will be automatically called after deserialization completes.
        /// </summary>
        public virtual void OnLoaded() { }

        //protected static T Load()
        //{
        //    return Deserialize();
        //}


        /// <summary>
        /// Deserializes xml data from a URL.
        /// </summary>
        /// <param name="url">URL of the xml file.</param>
        /// <param name="disableOnLoaded">Prevents the newly created instance's <see cref="OnLoaded"/> method from being
        /// called.</param>
        /// <returns>An instance of T with values loaded from the URL.</returns>
        public static T LoadFromUrl(string url = null, bool disableOnLoaded = false)
        {
            if (string.IsNullOrWhiteSpace(url))
                url = XmlUrl;

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException($"The provided '{typeof(T).Name}' URL was invalid.", nameof(url));
            }

            var xmlData = Network.DownloadStringFromUrl(url).GetAwaiter().GetResult();
            var instance = DeserializeFromString(xmlData);
            if (!disableOnLoaded)
                instance.OnLoaded();

            return instance;
        }

        ///// <summary>
        ///// First tries to load an XML from an online URL. If that fails, it next tries to load the XML from a file.
        ///// Finally, if that fails, it loads a new class instance using default values.
        ///// </summary>
        ///// <param name="url">The URL to load the file from. If not provided, the <see cref="XmlUrl"/> value will
        ///// instead be used.</param>
        ///// <param name="path">The path to load the file from. If not provided, the <see cref="XmlFilename"/> value will
        ///// instead be used.</param>
        ///// <returns>An instance of <see cref="T"/>.</returns>
        //public static T LoadXmlOnlineOrDefaults(string url = null, string path = null)
        //{

        //}

        /// <summary>
        /// Attempts to deserialize and load an instance of <see cref="T"/> from file. If this fails, a default instance
        /// of <see cref="T"/> will instead be created and returned.
        /// </summary>
        /// <param name="path">The path to load the file from. If not provided, the <see cref="XmlFilename"/> value will
        /// be used instead.</param>
        /// <param name="disableOnLoaded">Prevents the newly created instance's <see cref="OnLoaded"/> method from being
        /// called.</param>
        /// <param name="disableLoadFromFile">Prevents data from being loaded from file (which will result in defaults
        /// being loaded).</param>
        /// <returns>An instance of <see cref="T"/>.</returns>
        public static T LoadXmlOrDefaults(string path = null, 
                                          bool disableOnLoaded = false, 
                                          bool disableLoadFromFile = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = XmlPath;

            T instance = null;

            if (!disableLoadFromFile)
            {
                try
                {
                    // Try to load the XML file from storage
                    instance = Deserialize(path);
                    if (!disableOnLoaded)
                        instance.OnLoaded();

                    //return LoadFromFile<T>(path);
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case FileNotFoundException _:
                            Logger.Info($"The '{typeof(T).Name}' data file was not found.");
                            break;
                        case XmlException _:
                        case NullReferenceException _:
                        case InvalidOperationException _:
                            Logger.Warning($"The '{typeof(T).Name}' data file contains invalid data.");
                            RenameInvalidXml(path);
                            break;
                        default:
                            Logger.Error($"An unknown error occurred while loading the '{typeof(T).Name}' data " +
                                         $"file: {ex.Message}");
                            break;
                    }


                }
            }

            if (instance == null)
            {
                instance = GetDefaultInstance();
                if (!disableOnLoaded)
                    instance.OnLoaded();
            }

            return instance;
        }

        /// <summary>
        /// Creates an instance of <see cref="T"/>, calls <see cref="LoadDefaults"/> override, and then calls <see
        /// cref="OnLoaded"/> override (since some classes may depend on this method being called for executing
        /// initialization routines).
        /// </summary>
        /// <returns></returns>
        protected static T GetDefaultInstance()
        {
            var instance = (T)Activator.CreateInstance(typeof(T), true);
            instance?.LoadDefaults();
            return instance;
        }

        /// <summary>
        /// When <see cref="XmlFile{T}"/> creates a new, default instance of <see cref="T"/>, this method gives derived
        /// classes an opportunity to customize their default state.
        /// </summary>
        protected virtual void LoadDefaults() { }

        /// <summary>
        /// For classes that can pull updated data from the internet, this is the method they should override to perform
        /// the online update.
        /// </summary>
        public virtual void OnlineUpdate(bool ignoreAutoUpdateSetting = false) { }

        ///// <summary>
        ///// Deserializes xml data from file.
        ///// </summary>
        ///// /// <param name="path">Path to the xml file.</param>
        ///// <returns>An instance of T with values loaded from the file.</returns>
        //protected static T LoadFromFile<T>(string path)
        //{
        //    // Try to load and deserialize the XML file
        //    var serializer = new XmlSerializer(typeof(T));

        //    using (var fStream = new FileStream(path, FileMode.Open))
        //        return (T)serializer.Deserialize(fStream);

        //    //try
        //    //{
        //    //    if (string.IsNullOrEmpty(path))
        //    //    {
        //    //        throw new ArgumentException($"The {typeof(T).Name} data file path was invalid.", nameof(path));
        //    //        //path = (string)typeof(T).GetField("XML_PATH")?.GetValue(null);
        //    //    }

        //    //    // Try to load the plugin's offsets data file
        //    //    var serializer = new XmlSerializer(typeof(T));

        //    //    T instance;
        //    //    using (var fStream = new FileStream(path, FileMode.Open))
        //    //        instance = (T)serializer.Deserialize(fStream);

        //    //    if (instance == null)
        //    //        throw new NullReferenceException();

        //    //    instance.RunAfterDeserialize();
        //    //    return instance;
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    switch (ex)
        //    //    {
        //    //        case FileNotFoundException _:
        //    //            Logger.Info($"The {typeof(T).Name} data file was not found.");
        //    //            break;
        //    //        case XmlException _:
        //    //        case NullReferenceException _:
        //    //        case InvalidOperationException _:
        //    //            Logger.Warning($"The {typeof(T).Name} data file contains invalid data.");
        //    //            RenameInvalidXml(path);
        //    //            break;
        //    //        default:
        //    //            Logger.Error($"An unknown error occurred while loading the {typeof(T).Name} data file: {ex.Message}");
        //    //            break;
        //    //    }
        //    //}

        //    //return (T)Activator.CreateInstance(typeof(T), true);
        //}

        /// <summary>
        /// Used to backup/rename an existing XML file that contains invalid/unparseable data. The file is preserved so
        /// a user can fix the file if desired and rename it back to its original/valid name.
        /// </summary>
        protected static void RenameInvalidXml(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = XmlPath;

            try
            {
                var xmlNameNoExt = Path.GetFileNameWithoutExtension(path);
                var xmlExt = Path.GetExtension(path);
                var xmlDirectory = Path.GetDirectoryName(path) ?? "";
                string backupXmlPath = null;
                var count = 0;
                do
                {
                    if (count > 9)
                        throw new Exception($"No unused filename could be found for {XmlFilename}.");

                    var backupXmlName = $"{xmlNameNoExt}.invalid-{count}{xmlExt}";
                    backupXmlPath = Path.Combine(xmlDirectory, backupXmlName);
                    count += 1;
                } while (File.Exists(backupXmlPath));

                File.Move(path, backupXmlPath);
                Logger.Warning($"Successfully renamed invalid {XmlFilename} to '{backupXmlPath}'.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to backup/rename invalid {XmlFilename}: {ex.Message}");
            }
        }

        /// <summary>
        /// Deserialize an XML string. 
        /// </summary>
        /// <remarks>Makes use of 'XmlCallbackSerializer' which allows for a callback function to be triggered after
        /// deserialization is complete. The deserialized class must be marked as '[Serializable]' and must implement a
        /// function such as 'internal void OnDeserialized(StreamingContext context)'. This function must be marked with
        /// the attribute '[OnDeserialized]'.</remarks>
        /// <typeparam name="T">Class type.</typeparam>
        /// <param name="value">The XML string containing the deserialized class instance.</param>
        /// <returns>A new class instance built from the serialized XML string.</returns>
        protected static T DeserializeFromString(string value)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(ms);
            }
        }

        /// <summary>
        /// Deserialize a class instance from an XML file.
        /// </summary>
        /// <remarks>Makes use of 'XmlCallbackSerializer' which allows for a callback function to be triggered after
        /// deserialization is complete. The deserialized class must be marked as '[Serializable]' and must implement a
        /// function such as 'internal void OnDeserialized(StreamingContext context)'. This function must be marked with
        /// the attribute '[OnDeserialized]'.</remarks>
        /// <typeparam name="T">Class type.</typeparam>
        /// <param name="path">The file path of the XML file to deserialize.</param>
        /// <returns></returns>
        protected static T Deserialize(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = XmlPath;

            using (var stream = File.OpenRead(path))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stream);
            }
        }

        #endregion
    }
}
