using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

// TODO: Doesn't read information that comes after the main content. (charaName, etc in CommandList) - "uk_loads"?
// TODO: Update Pointer to footer and anything else that might not have been updated
namespace SFVAnimationsEditor.Model
{
    public class UassetFile
    {
        private const int BYTE_GROUP_SIZE = 4;

        private const int LOC_PTR_CONTENT = 0x18;
        private const int LOC_PTR_FOOTER   = 0x93;

        public ObservableCollection<StringProperty> StringList { get; set; }
        public DeclarationBlock Declaration { get; set; }
        public UnknownList1Block UnknownList1 { get; set; } // Content metadata?
        public ImportBlock Imports { get; set; }
        public UkDepends UkDepends { get; set; }
        // public __?__ UkLoads { get; set; }

        public long PtrFooter;
        public long PtrNoneString;
        public long NoneIndex;

        public int PreContentSize; // alternatively, pointer to start of content
        public int Unknown1;
        public int StringListCount;
        public int StringListPtr;
        public int UnknownList1Count;
        public int UnknownList1Ptr;
        public int DeclareCount;
        public int DeclarePtr;
        public int ImportsListPtr;
        public int UkDependsCount;
        public long UkDependsPtr;
        public int ContentCount;
        public int Unknown2;

        public byte[] Checksum;
        public byte[] FooterBytes;

        public delegate object PropertyTypeDelegate(BinaryReader br);


        public virtual void ReadUasset(ref BinaryReader br)
        {
            br.BaseStream.Seek(LOC_PTR_CONTENT, SeekOrigin.Begin); // skip to declare block's size location
            PreContentSize = br.ReadInt32();

            // Read "None"
            br.ReadBytes(br.ReadInt32()); // (compact way to read this part) We don't do anything with None string except store the end position
            PtrNoneString = br.BaseStream.Position;
            
            // Store Unknown1
            Unknown1 = br.ReadInt32();

            // Read String List metadata
            StringListCount = br.ReadInt32();
            StringListPtr   = br.ReadInt32();

            // Read and store unknown list. What is this list? Content Metadata?
            UnknownList1Count = br.ReadInt32();
            UnknownList1Ptr   = br.ReadInt32();
            
            // Read Declaration metadata
            DeclareCount  = br.ReadInt32();
            DeclarePtr    = br.ReadInt32();

            // Read Imports list metadata
            ////br.BaseStream.Seek(PtrNoneString + 0x1C, SeekOrigin.Begin);
            ImportsListPtr = br.ReadInt32();

            UkDependsCount = br.ReadInt32();
            UkDependsPtr = ReadInt32AndZero(br); // int64? or just another int32 like ImportsListPtr?
            
            Checksum = br.ReadBytes(0x10);

            int unk1 = br.ReadInt32();

            if (unk1 != 0x1)
            {
                Console.WriteLine($"Expected to read a 1 after the checksum! Actual value was {unk1}");
                return;
            }

            ContentCount = br.ReadInt32();

            var strListCountDupe = br.ReadInt32();
            if (strListCountDupe != StringListCount)
            {
                Console.WriteLine($"Expected to read the string list count again! Actual value was 0x{strListCountDupe:X} ({strListCountDupe})");
                return;
            }

            // Read 0x16 zeros
            for (var i = 0; i < 0x16; i++)
                if (br.ReadByte() != 0)
                {
                    Console.WriteLine($"Expected a zero at @{br.BaseStream.Position-1} but found something else!");
                    return;
                }

            // Read unknown int #2
            Unknown2 = br.ReadInt32();

            // Read "uk_loads"
            ReadZero(br); // TODO: Until I have a file that has some "loads" in it, I can't really do anything with it.
            ReadZero(br);

            // Read UkDependsPtr again (0xACE in DA_RYU_Trial_Vol1.uasset)
            unk1 = br.ReadInt32();
            if (unk1 != UkDependsPtr)
            {
                Console.WriteLine($"Expected to read the uk_depends Pointer again! Actual value was 0x{unk1:X} ({unk1})");
                //return;
            }

            // Read pointer to footer
            if (br.BaseStream.Position != LOC_PTR_FOOTER)
                br.BaseStream.Seek(LOC_PTR_FOOTER, SeekOrigin.Begin);
            PtrFooter = br.ReadInt64();

            ReadZero(br);
            ReadZero(br);

            // Read Lists
            ReadStringList(br);
            ReadDeclaration(br, DeclareCount, DeclarePtr);
            ReadUnknownList1(br); // Content metadata?
            ReadImports(br);
            ReadUkDepends(br);

            // Store footer bytes
            br.BaseStream.Seek(PtrFooter, SeekOrigin.Begin);
            FooterBytes = br.ReadBytes(4);

            //Start reading Uasset content...
            br.BaseStream.Seek(PreContentSize, SeekOrigin.Begin);
            ReadUassetContent(ref br);
        }

        public void ReadUassetContent(ref BinaryReader br)
        {
            // TODO: Store content!

            Console.WriteLine("\nReading Main Content..." +
                             $"\n\tBinaryReader position: {br.BaseStream.Position} (0x{br.BaseStream.Position:X})");

            // Read Content struct
            var attr = br.ReadInt32();
            var reference = StringList[attr];
            ReadZero(br);

            var propType = br.ReadByte(); // get prop type for console output
            br.BaseStream.Seek(-1, SeekOrigin.Current);

            var propertyTypeDelegate = ReadPropertyType(br);

            ////if (propertyTypeDelegate == ReadVoid)
            ////{
            ////    HandleReadVoid(br, attr);
            ////}

            var byteSize = ReadInt32AndZero(br);

            Console.WriteLine($"Reading {reference} (0x{attr:X})\n\tType: {StringList[propType]} (0x{propType:X})\n\tSize: {byteSize} bytes (0x{byteSize:X})");

            var content = propertyTypeDelegate(br);

            Console.WriteLine($"\nFinished reading uasset!");

        }

        public void WriteUasset(ref BinaryReader br, ref BinaryWriter bw)
        {
            // Add any new strings to the String List!
            ////UpdateStringList();

            // Proceed to write the UAsset

            bw.Write(br.ReadBytes(LOC_PTR_CONTENT));

            // Skip this integer for now, the rest must be calculated first...
            bw.Write(0);
            br.ReadInt32(); // reader must catch up
            
            bw.Write(br.ReadBytes(9)); // "None" bytes
            bw.Write(br.ReadBytes(4)); // unknown bytes

            bw.Write(StringList.Count);
            bw.Write(StringListPtr);

            bw.Write(UnknownList1.Count);
            var ptrLoc_UnknownList1 = (int)bw.BaseStream.Position; // store ptr location for now; write address when it's determined.
            bw.Write(0);

            bw.Write(Declaration.Count);
            var ptrLoc_Declaration = (int)bw.BaseStream.Position;
            bw.Write(0);
            
            // TODO: Needs testing! Imports Address might be a long. The UkDepends.Count might be written at the UkDepends.Address!
            var ptrLoc_Imports = (int)bw.BaseStream.Position;
            bw.Write(0L);
            //var ptrLoc_UkDepends = (int)bw.BaseStream.Position;
            bw.Write(0L);
            
            bw.Write(Checksum);

            bw.Write(1);
            bw.Write(ContentCount); // might be UnknownList1.Count
            bw.Write(StringList.Count);

            byte zeroByte = 0;
            for (var i = 0; i < 0x16; i++)
                bw.Write(zeroByte);

            bw.Write(Unknown2);

            // TODO: Write the UkLoads!
            //bw.Write(UkLoads.GetBytes());
            bw.Write(0); // TODO: Until I have a file that has some "loads" in it, I can't really do anything with it. WydD reads array content of StringProperty

            bw.Write(0);

            var ptrLoc_2_UkDepends = (int)bw.BaseStream.Position;
            bw.Write(0L);

            var ptrLoc_2_Footer = (int)bw.BaseStream.Position;
            bw.Write(0L);
            
            bw.Write(0);

            WriteStringList(bw);
            WriteDeclaration(bw);
            WriteUnknownList1(bw);
            WriteImports(bw);
            WriteUkDepends(bw);

            // get pre-content data size, go to the pointer location, write it, and go back to where we were
            var preContentSize = (int)bw.BaseStream.Position;
            bw.Seek(LOC_PTR_CONTENT, 0);
            bw.Write(preContentSize);
            bw.Seek(preContentSize, 0);

            ////WriteUassetContent(ref br, ref bw);

            // write post-content data
            bw.Write(0);

            // store position as location of footer pointer
            PtrFooter = bw.BaseStream.Position;


            // Write pointers to sections at their 'pointer locations'
            bw.Seek(ptrLoc_UnknownList1, 0);
            bw.Write(UnknownList1.Address);

            bw.Seek(ptrLoc_Declaration, 0);
            bw.Write(Declaration.Address);

            bw.Seek(ptrLoc_Imports, 0);
            bw.Write(Imports.Address);
            bw.Write(UkDepends.Address); // UkDepends comes immediately after Imports

            // write those weird second instances of the address
            bw.Seek(ptrLoc_2_UkDepends, 0);
            bw.Write(UkDepends.Address);

            bw.Seek(ptrLoc_2_Footer, 0);
            bw.Write(PtrFooter);

            bw.Seek(LOC_PTR_FOOTER, 0);
            bw.Write(PtrFooter); // NOTE: May need to be int, if long messes things up.
            // write footer
            bw.Seek((int)PtrFooter, 0);
            bw.Write(FooterBytes);
        }

        ////internal abstract void WriteUassetContent(ref BinaryReader br, ref BinaryWriter bw);
        ////internal abstract void UpdateStringList();

        internal void ReadStringList(BinaryReader br)
        {
            if (br.BaseStream.Position != StringListPtr)
                br.BaseStream.Seek(StringListPtr, SeekOrigin.Begin);

            StringList = new ObservableCollection<StringProperty>();
            Console.WriteLine($"\nReading String List ({StringListCount} items): ");

            int stringSize, strIndex;
            string str;

            while (StringList.Count < StringListCount)
            {
                stringSize = br.ReadInt32();
                StringList.Add(new StringProperty(br.ReadChars(stringSize - 1)));
                br.BaseStream.Seek(1, SeekOrigin.Current); // skip the extra byte of the string

                strIndex = StringList.Count - 1;
                Console.WriteLine("0x" + (strIndex).ToString("X2") + "\t= " + StringList[strIndex].Value);
            }

            NoneIndex = StringList.IndexOf(StringList.Where(s => s.Value == "None").First());
        }

        internal void ReadDeclaration(BinaryReader br, int count, int address)
        {
            Declaration = new DeclarationBlock(count, address);
            Console.WriteLine($"\nReading Declaration Block ({Declaration.Count} items): ");

            br.BaseStream.Seek(Declaration.Address, SeekOrigin.Begin);

            DeclarationItem di;
            for (var i = 0; i < Declaration.Count; i++)
            {
                di = new DeclarationItem();
                di.ReadItems(br, StringList);
                di.Id = i;

                Console.WriteLine(//"\n\t  Finished reading Declaration Item:" +
                                 $"\n\t    Namespace:\t{di.Namespace}" + 
                                 $"\n\t    Type:\t{di.Type}" + 
                                 $"\n\t    Name:\t{di.Name}");

                Declaration.Items[i] = di;
            }
        }

        internal void ReadUnknownList1(BinaryReader br)
        {
            Console.WriteLine("\nReading Unknown List 1: ");

            UnknownList1 = new UnknownList1Block
            {
                Count = UnknownList1Count,
                Address = UnknownList1Ptr
            };

            UnknownList1.ReadList(br, StringList);

            Console.WriteLine("\nFinished reading Unknown List 1.\n");
        }

        internal void ReadImports(BinaryReader br)
        {
            Console.WriteLine("\nReading Imports:\t(<ID> - <Name>)");
            
            br.BaseStream.Seek(ImportsListPtr, SeekOrigin.Begin);

            var importsListCount = br.ReadInt32();

            Imports = new ImportBlock {
                Address = ImportsListPtr,
                Count = importsListCount
            };

            int val;
            for (var i = 0; i < Imports.Count; i++)
            {
                Imports.Items.Add(-br.ReadInt32() - 1);
                val = Imports.Items[i];
                Console.Write($"\t{Imports.Items[i]}");
                Console.WriteLine((val >= 0) ? $" - {Declaration.Items[val].Name}" : "");
            }
        }

        internal void ReadUkDepends(BinaryReader br)
        {
            if (br.BaseStream.Position != UkDependsPtr)
                br.BaseStream.Seek(UkDependsPtr, 0);

            UkDepends = new UkDepends {
                Count = br.ReadInt32(),
                Address = UkDependsPtr
            };

            if (UkDepends.Count != 0)
                System.Windows.MessageBox.Show("There are actually items in UkDepends! Notify me!");

            for (var i = 0; i < UkDepends.Count; i++)
            {
                UkDepends.Items.Add(br.ReadInt32()); // Idk if this works without a file that has ukdepends
            }
        }

        internal void WriteStringList(BinaryWriter bw)
        {
            for (var i = 0; i < StringList.Count; i++)
            {
                bw.Write(StringList[i].GetBytes());
            }
        }

        internal void WriteDeclaration(BinaryWriter bw)
        {
            Declaration.Address = (int)bw.BaseStream.Position;

            var stringListStrings = StringList.Select(s => s.Value).ToList();

            for (var i = 0; i < Declaration.Count; i++)
            {
                Declaration.Items[i].WriteItems(bw, stringListStrings);
            }
        }

        internal void WriteUnknownList1(BinaryWriter bw)
        {
            UnknownList1.WriteList(bw, StringList);
        }

        internal void WriteImports(BinaryWriter bw)
        {
            Imports.Address = bw.BaseStream.Position;

            bw.Write(Imports.Count);
            for (var i = 0; i < Imports.Count; i++)
            {
                if (Imports.Items[i] == 0) {
                    bw.Write(0);
                    continue;
                }

                var importId = -Imports.Items[i] - 1;
                bw.Write(importId); // TODO: Derive this value.
            }
        }

        internal void WriteUkDepends(BinaryWriter bw)
        {
            UkDepends.Address = bw.BaseStream.Position;

            bw.Write(UkDepends.Count);
            for (var i = 0; i < UkDepends.Count; i++)
            {
                bw.Write(UkDepends.Items[i]);
            }
        }


        internal virtual PropertyTypeDelegate ReadPropertyType(BinaryReader br)
        {
            var propType = ReadInt32AndZero(br);
            string propertyTypeString = StringList[propType].Value;
            Console.WriteLine($"\nFound property type \"{propertyTypeString}\" at position 0x{br.BaseStream.Position-8:X}.");

            switch (propertyTypeString)
            {
                case "StructProperty":
                    return ReadStruct;
                case "ArrayProperty":
                    return ReadArray;
                case "TextProperty":
                    return ReadText;
                case "IntProperty":
                    return ReadInt;
                ////case "ByteProperty":
                ////    return ReadByte;
                case "BoolProperty":
                    return ReadBool;
                case "ObjectProperty":
                    return ReadObject;
                case "StrProperty":
                    return ReadString;
                default:
                    var badText = $"Attempted to read unknown property! (0x{propType:X2}: {propertyTypeString})";
                    // TODO: Roll back and try again.
                    Console.WriteLine(badText);
                    ////PressAnyKey();
                    ////throw new InvalidDataException(badText);
                    return ReadVoid;
            }
        }

        internal Dictionary<string, object> ReadStruct(BinaryReader br)
        {
            var results = new Dictionary<string, object>();
            while (true)
            {
                Console.WriteLine("\nReading StructProperty...");
                #region Get Attribute Name String Index

                var attr = ReadInt32AndZero(br);
                var reference = StringList[attr].Value;

                Console.WriteLine("\tAttribute: \"{0}\" (0x{1:X2})", reference, attr);
                
                #endregion

                if (reference == "None")
                {
                    // end of struct!
                    Console.WriteLine("Encountered \"None\"! (end of array item) Breaking...");
                    break; // break loop without adding a new item
                }

                if (reference == "StringAssetReference")
                {
                    results[reference] = ReadString(br);
                    Console.WriteLine("Found StringAssetReference! (End of command list)"); // TODO: Finish this.

                    return results;
                }

                // if it's safe to read, show these
                var propType = ReadInt32AndZero(br);
                var previewSize = ReadInt32AndZero(br);
                Console.WriteLine("\tProperty Type: \"{0}\" (0x{1:X2} = {1:D2})", StringList[propType], propType);
                Console.WriteLine("\tSize: {0} bytes", (StringList[propType].Value == "ByteProperty") ? $"{previewSize} or {previewSize*2}" : previewSize.ToString());

                if (previewSize == 0) { continue; }
                RollBack(br, 2, true);

                // Get Property Type
                var readType = ReadPropertyType(br);

                ////if (readType == ReadVoid)
                ////{
                ////    HandleReadVoid(br, attr);
                ////}

                // Get Size (in bytes)
                var size = ReadInt32AndZero(br);

                results[reference] = readType.Invoke(br);
            }

            return results;
        }

        internal ArrayProperty ReadArray(BinaryReader br)
        {
            Console.WriteLine("\nReading Array property...");

            ArrayProperty arrayProp;

            var propType = ReadInt32AndZero(br); // preview property type

            Console.WriteLine($"\tProperty Type: \"{StringList[propType]}\" (0x{propType:X2} = {propType})");
            Console.WriteLine($"\tNumber of Items: {br.PeekChar()}");

            RollBack(br, 1, true); // go back to where we should be

            var readType = ReadPropertyType(br);

            var count = br.ReadInt32();

            arrayProp = new ArrayProperty
            {
                PropertyType = propType,
                //Count = count,
                Items = new ObservableCollection<dynamic>()
            };

            // read items in array
            for (var i = 0; i < count; i++)
            {
                // add item to array
                var item = readType.Invoke(br);

                arrayProp.Items.Add(item);
            }

            return arrayProp;
        }

        internal BoolProperty ReadBool(BinaryReader br)
        {
            Console.WriteLine("\nReading Bool Property...");
            var result = new BoolProperty { Value = br.ReadUInt64() };

            Console.WriteLine($"\t{result.Value} = {result.Value != 0}");

            return result;
        }

        internal TextProperty ReadText(BinaryReader br)
        {
            Console.WriteLine("\nReading Text Property...");
            var result = new TextProperty();

            // Always skip 9 bytes of zeroes (4 + 1 + 4)
            ReadZero(br);

            byte next = br.ReadByte();
            if (next == 0)
            {
                ReadZero(br); // skip 4 more bytes
                result.Uuid = ReadString(br);    // read UUID
                result.Content = ReadString(br); // read Content/Value
            }
            else
                result.Id = next; // it's possible that the 5th byte can be non-zero? In which case, it's the ID.  @A9A, it's 0xFF (-1)! Followed immediately by "None" index

            Console.WriteLine($"\tUUID: {result.Uuid}\n" +
                              $"\tContent: {result.Content}");
            if (result.Id != 0)
                Console.WriteLine($"\t(Id: {result.Id})");

            return result;
        }

        internal IntProperty ReadInt(BinaryReader br)
        {
            var ip = new IntProperty { Value = br.ReadInt32() };
            Console.WriteLine($"\tIntProperty Value: {ip.Value} (0x{ip.Value:X}");

            return ip;
        }

        internal ObjectProperty ReadObject(BinaryReader br)
        {
            ObjectProperty obj = new ObjectProperty();

            var unparsedId = br.ReadInt32();

            obj.SetId(unparsedId);

            if (unparsedId == 0)
            {
                Console.WriteLine("Read null object:");
                obj.Name = null;
                return obj;
            }
            else if (unparsedId > 0)
            {
                Console.WriteLine($"Unknown object! Object index was 0x{unparsedId:X2}:");
                obj.Name = null;
                obj.Id = unparsedId;
            }
            else
            {
                obj.Name = Declaration.Items[obj.Id].Name;
                Console.WriteLine($"Read object:");
            }
            Console.WriteLine($"\tID:\t{obj.Id}\n" + 
                $"\tName:\t{obj.Name}\n");

            return obj;
        }

        ////internal abstract object ReadByte(BinaryReader br);

        internal string ReadString(BinaryReader br)
        {
            var size = br.ReadInt32();
            if (size == 0)
            {
                Console.WriteLine("\tStringProperty was empty.\n");
                return "";
            }

            var str = new string(br.ReadChars(size), 0, size - 1);
            Console.WriteLine($"\t\"{str}\"");
            return str;
        }

        internal string ReadVoid(BinaryReader br) { return ""; }

        internal void ReadZero(BinaryReader reader)
        {
            var zero = reader.ReadInt32();

            if (zero != 0)
            {
                Console.WriteLine("\nExpected zero but found something else! Quitting...");
                //PressAnyKey();

                throw new InvalidDataException("Expected zero!"); // Should read zero here.
            }
        }

        // TODO: UNUSED
        ////internal abstract void HandleReadVoid(BinaryReader br, int propType);

        
        public int ReadInt32AndZero(BinaryReader br)
        {
            var i = br.ReadInt32();
            ReadZero(br);

            return i;
        }

        public void RollBack(BinaryReader br, int times = 1, bool silent = false)
        {
            br.BaseStream.Seek(-8 * times, SeekOrigin.Current);
            if (!silent) Console.WriteLine($"Rolled back! Now at position 0x{br.BaseStream.Position:X}");
        }
    }


    public abstract class UAssetProperty
    {
        public abstract byte[] GetBytes();
    }

    public class ArrayProperty : UAssetProperty
    {
        /// <summary>
        /// The type of property that will be held.
        /// </summary>
        public long PropertyType { get; set; }
        /// <summary>
        /// The number of items in the array.
        /// </summary>
        public int Count { get => Items.Count; }
        /// <summary>
        /// The items in the array.
        /// </summary>
        public ObservableCollection<dynamic> Items { get; set; } = new ObservableCollection<dynamic>();


        public override byte[] GetBytes()
        {
            throw new NotImplementedException();
        }

        public byte[] GetBytes(long noneIndex)
        {
            List<byte> resultBytes = new List<byte>();
            List<byte> itemsBytes   = new List<byte>();

            // call GetBytes() on each item
            for (var i = 0; i < Items.Count; i++)
            {
                itemsBytes.AddRange(((UAssetProperty)Items[i]).GetBytes());
                itemsBytes.AddRange(BitConverter.GetBytes(noneIndex));
            }

            // add prop type
            resultBytes.AddRange(BitConverter.GetBytes(PropertyType));
            resultBytes.AddRange(BitConverter.GetBytes(Items.Count));
            resultBytes.AddRange(itemsBytes);

            return resultBytes.ToArray();
        }
    }

    public class BoolProperty : UAssetProperty
    {
        public ulong Value { get; set; }

        public override byte[] GetBytes()
        {
            throw new NotImplementedException();
        }
    }

    public class IntProperty : UAssetProperty
    {
        public int Value { get; set; }


        public override byte[] GetBytes()
        {
            var resultBytes = new List<byte>();
            // get bytes of Value
            byte[] valueBytes = BitConverter.GetBytes(Value);
            // add value's size in bytes
            resultBytes.AddRange(BitConverter.GetBytes(valueBytes.Length));
            resultBytes.AddRange(valueBytes);

            return resultBytes.ToArray();
        }
    }

    public class TextProperty : UAssetProperty
    {
        public string Uuid { get; set; }
        public string Content { get; set; }
        public int Id { get; set; }


        public override byte[] GetBytes()
        {
            var resultBytes = new List<byte>();

            var zeroBytes = BitConverter.GetBytes(0);

            resultBytes.AddRange(zeroBytes);
            resultBytes.Add((byte)Id);

            if (Id == 0)
            {
                resultBytes.AddRange(zeroBytes);
                // add uuid
                resultBytes.AddRange(GetStringBytes(Uuid ?? GetNewUuid()));
                // add content
                resultBytes.AddRange(GetStringBytes(Content));
            }

            return resultBytes.ToArray();
        }

        private IEnumerable<byte> GetStringBytes(string s)
        {
            var resultBytes = new List<byte>();

            // reverse ReadString
            // add size of string + 1
            resultBytes.AddRange(BitConverter.GetBytes(s.Length + 1));
            // add characters + 1 empty byte
            resultBytes.AddRange(System.Text.Encoding.UTF8.GetBytes(s));
            resultBytes.Add(new byte());

            return resultBytes;
        }

        public static string GetNewUuid()
        {
            return Guid.NewGuid().ToString("N").ToUpper();
        }
    }

    public class StringProperty : UAssetProperty
    {
        public string Value { get; set; } = "";


        public StringProperty(string value = "")
        {
            Value = value;
        }

        public StringProperty(char[] value)
        {
            if (value == null) return;

            Value = new string(value);
        }

        public override byte[] GetBytes()
        {
            var resultBytes = new List<byte>();

            resultBytes.AddRange(BitConverter.GetBytes(Value.Length + 1));
            resultBytes.AddRange(System.Text.Encoding.UTF8.GetBytes(Value));
            resultBytes.Add(new byte());

            return resultBytes.ToArray();
        }
    }

    public class ObjectProperty : UAssetProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Parse and set Id.
        /// </summary>
        /// <param name="value">The unparsed ID.</param>
        public void SetId(int value)
        {
            Id = -value - 1;
        }


        public override byte[] GetBytes()
        {
            // return id bytes, 
                // TODO: ? - update the id if necessary? (Get index of Name from string list?)
            return BitConverter.GetBytes(-Id - 1);
        }
    }

    public abstract class ByteProperty : UAssetProperty { }

    public class DeclarationBlock
    {
        public int Count;
        public int Address;

        public DeclarationItem[] Items;


        public DeclarationBlock(int count, int address)
        {
            Count = count;
            Address = address;

            Items = new DeclarationItem[Count];
        }
    }

    public class DeclarationItem
    {
        public int    Id; // is depends derived from this?
        public string Namespace;
        public string Type;
        public string Name;

        public int    Depends;

        public int[] Items = new int[7];


        public void ReadItems(BinaryReader br, IList<StringProperty> stringList)
        {
            Console.Write("\n\tReading Declare Block items' IDs:    ");

            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = br.ReadInt32();
                Console.Write($"{Items[i]}  ");
            }

            // TODO: We're just copying the depends number. How do we derive it?
            //      It probably requires storing a reference to the Id of the item and using the reference's Id to convert it back.
            //      Also, there are depends with a value of 0 when they don't depend on anything, so those could just store a null reference.

            Namespace = stringList[Items[0]].Value;
            Type      = stringList[Items[2]].Value;
            Name      = stringList[Items[5]].Value;

            Depends = Items[4]; // possibly temporary; Depends List ID of the item
        }

        public void WriteItems(BinaryWriter bw, IList<string> stringList)
        {
            /**
             * 0 = "namespace"      (e.g. - "/Script/CoreUObject")
             * 1 = zero
             * 2 = "type"           (e.g. - "Class")
             * 3 = zero
             * 4 = "depends" id     (refers to ID of another item in the Declaration) (negative number, convert it to ID in the usual way?)
             * 5 = "name"           (e.g. - "KWBattleCharaTrialDataAsset")
             * 6 = zero             (in Trial_Vol1 uassets, this is "11" when the trial is not numbered [e.g. - TRIAL vs TRIAL_09])
             */
            bw.Write(stringList.IndexOf(Namespace));
            bw.Write(0);
            bw.Write(stringList.IndexOf(Type));
            bw.Write(0);
            bw.Write(Depends);
            bw.Write(stringList.IndexOf(Name));
            bw.Write(Items[6]); 
        }
    }

    public class UnknownList1Block
    {
        public int Count;
        public int Address;

        public List<UnknownList1Item> Items = new List<UnknownList1Item>();

        public void ReadList(BinaryReader br, IList<StringProperty> stringList)
        {
            if (br.BaseStream.Position != Address)
                br.BaseStream.Seek(Address, 0);

            // Read list of list of ints
            for (var i = 0; i < Count; i++)
            {
                var item = new UnknownList1Item();

                item.ReadUnknownList1Item(br, stringList);
                
                Items.Add(item);
            }
        }

        public void WriteList(BinaryWriter bw, IList<StringProperty> stringList)
        {
            Address = (int)bw.BaseStream.Position;
            
            // Write list of list of ints
            for (var i = 0; i < Count; i++)
            {
                Items[i].WriteUnknownList1Item(bw, stringList);
            }
        }
    }

    public class UnknownList1Item
    {
        public const int ITEM_COUNT = 17;

        public int Id;
        public string Name;
        public string Namespace;
        public int Size;
        public int PtrToContent;

        public List<int> Items = new List<int>(ITEM_COUNT);


        public void ReadUnknownList1Item(BinaryReader br, IList<StringProperty> stringList)
        {
            for (var i = 0; i < ITEM_COUNT; i++)
                Items.Add(br.ReadInt32());

            // 0 3 5 6 7 16
            Id = -Items[0] - 1;
            Name = stringList[Items[3]].Value;
            Namespace = stringList[Items[5]].Value;
            Size = Items[6];
            PtrToContent = Items[7];

            Console.WriteLine($"\tId:\t{Id}\n" +
                $"\tName:\t{Name}\n" +
                $"\tNamespace:\t{Namespace}\n" +
                $"\tSize:\t{Size}\n" +
                $"\tPointer To Content:  @{PtrToContent:X}");
        }

        public void WriteUnknownList1Item(BinaryWriter bw, IList<StringProperty> stringList)
        {
            for (var i = 0; i < Items.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        bw.Write(-Id - 1);
                        break;
                    case 3:
                        bw.Write(stringList.IndexOf(stringList.Where(s => s.Value == Name).First()));
                        break;
                    case 5:
                        bw.Write(stringList.IndexOf(stringList.Where(s => s.Value == Namespace).First()));
                        break;
                    case 6:
                        bw.Write(Size);
                        break;
                    case 7:
                        bw.Write(PtrToContent);
                        break;
                    case 16:
                    default:
                        bw.Write(Items[i]);
                        break;
                }
            }
        }
    }

    public class ImportBlock
    {
        public int Count;
        public long Address;

        public List<int> Items = new List<int>();
    }

    public class UkDepends
    {
        public int Count;
        public long Address;

        public List<int> Items = new List<int>();
    }


    [Serializable] public class ParseException : Exception { }
}
