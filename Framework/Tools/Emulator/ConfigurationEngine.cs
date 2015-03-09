////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Reflection;
using Microsoft.SPOT.Emulator;

namespace Microsoft.SPOT.Emulator
{
    public class ConfigurationEngine
    {
        Dictionary<String, String> _typesLookup;
        Assembly _defaultAssembly;
        Emulator _emulator;

        internal ConfigurationEngine( Emulator emulator )
        {
            _emulator = emulator;
            _typesLookup = new Dictionary<String, String>();
            _defaultAssembly = GetType().Assembly;
        }
        
        internal void ConfigureEmulator(XmlReader reader)
        {
            while (true)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    switch (reader.Name.ToLower())
                    {
                        case "import":
                            reader.MoveToAttribute( "filename" );
                            _emulator.ApplyConfig(reader.Value);
                            break;
                        case "types":
                            ProcessTypes( reader );
                            break;
                        case "emulatorcomponents":
                            ProcessEmulatorComponents( reader );
                            break;
                        default:
                            throw new Exception( "Unknown element: " + reader.Name );
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void ProcessTypes( XmlReader reader )
        {
            while (true)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    _typesLookup.Add( reader.Name, reader.ReadElementContentAsString() );
                }
                else
                {
                    break;
                }
            }
        }

        private void ProcessEmulatorComponents( XmlReader reader )
        {
            while (true)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    Type type = ResolveEmulatorComponentType( reader.Name );
                    XmlReader ecConfig = reader.ReadSubtree();

                    ProcessEmulatorComponent( type, ecConfig );

                    ecConfig.Close();
                }
                else
                {
                    break;
                }
            }
        }

        [Flags]
        private enum IdAttributes
        {
            None = 0,
            ComponentId = 1,
            ReplacesId = 2,
            UpdatesId = 4,
            RemovesId = 8
        }

        private EmulatorComponent ProcessEmulatorComponent( Type type, XmlReader config )
        {
            String componentId = null, replacesId = null, updatesId = null, removesId = null;
            IdAttributes attr = IdAttributes.None;

            config.MoveToContent();

            if (config.MoveToAttribute( "id" ))
            {
                componentId = config.Value;

                if (componentId == "")
                {
                    componentId = null;
                }
                else
                {
                    attr |= IdAttributes.ComponentId;
                }
            }
            if (config.MoveToAttribute( "replaces" ))
            {
                replacesId = config.Value;
                attr |= IdAttributes.ReplacesId;
            }
            if (config.MoveToAttribute( "updates" ))
            {
                updatesId = config.Value;
                attr |= IdAttributes.UpdatesId;
            }
            if (config.MoveToAttribute( "removes" ))
            {
                removesId = config.Value;
                attr |= IdAttributes.RemovesId;
            }

            config.MoveToElement();

            EmulatorComponent ec = null;

            switch (attr)
            {
                case IdAttributes.None:
                case IdAttributes.ComponentId:
                    ec = CreateEmulatorComponent( type, componentId, config );
                    break;
                case IdAttributes.ReplacesId:
                case IdAttributes.ReplacesId | IdAttributes.ComponentId:
                    ec = ReplaceEmulatorComponent( type, componentId, replacesId, config );
                    break;
                case IdAttributes.UpdatesId:
                case IdAttributes.UpdatesId | IdAttributes.ComponentId:
                    ec = UpdateEmulatorComponent( type, componentId, updatesId, config );
                    break;
                case IdAttributes.RemovesId:
                    RemoveEmulatorComponent( type, removesId );
                    break;
                default:
                    throw new Exception( "Invalid syntax while parsing EmulatorComponent of type" + type.ToString() + "." );
            }

            return ec;
        }

        private EmulatorComponent CreateEmulatorComponent( Type type, String componentId, XmlReader config )
        {
            if (componentId != null && _emulator.FindComponentById( componentId ) != null)
            {
                throw new Exception( "Component, " + componentId + ", already existed." );
            }

            // Create the component and configure it
            EmulatorComponent component = (EmulatorComponent)Activator.CreateInstance( type );

            component.SetEmulator( _emulator );

            if (String.IsNullOrEmpty( componentId ) == false)
            {
                component.ComponentId = componentId;
            }

            component.Configure( config );

            // Make sure the component is unique (there isn't any existing components that this new component will replace)
            EmulatorComponent replaceable = _emulator.FindReplaceableComponent( component );
            if (replaceable != null)
            {
                if (replaceable.CreatedByConfigurationFile)
                {
                    throw new Exception( "Another component, " + replaceable.ComponentId + ", already exists." );
                }
                else
                {
                    // if the component is generated by default, then we won't enforce the replaceId requirement, and replace it.
                    _emulator.UnregisterComponent( replaceable );
                }
            }

            _emulator.RegisterComponent( component );

            return component;
        }

        private EmulatorComponent ReplaceEmulatorComponent( Type type, String componentId, String replacedId, XmlReader config )
        {
            EmulatorComponent replaced = _emulator.FindComponentById( replacedId );

            if (replaced == null)
            {
                throw new Exception( "Cannot find the component, " + replacedId + ", to be replaced." );
            }

            if (componentId == null)
            {
                componentId = replacedId;
            }

            // Make sure the new componentId isn't taken already
            EmulatorComponent idMatched = _emulator.FindComponentById( componentId );
            if (idMatched != null)
            {
                if (idMatched != replaced)
                {
                    throw new Exception( "The Component ID, " + componentId + ", has already been used." );
                }
                else if (idMatched.LinkedBy != null)
                {
                    throw new Exception( "Cannot replace the EmulatorComponent " + idMatched.ComponentId + " here because it's linked elsewhere." );
                }
            }

            // Create the component and configure it
            EmulatorComponent component = (EmulatorComponent)Activator.CreateInstance( type );

            component.ComponentId = componentId;

            component.SetEmulator(_emulator);
            component.Configure( config );

            if (replaced.IsReplaceableBy( component ) == false)
            {
                throw new Exception( "The new component cannot be used to replace the old one." );
            }

            _emulator.UnregisterComponent( replaced );

            Debug.Assert( _emulator.FindReplaceableComponent( component ) == null );

            _emulator.RegisterComponent( component );

            return component;
        }

        private EmulatorComponent UpdateEmulatorComponent( Type type, String componentId, String updatedId, XmlReader config )
        {
            EmulatorComponent updated = _emulator.FindComponentById( updatedId );

            if (updated == null)
            {
                throw new Exception( "Cannot find component " + updatedId + "." );
            }

            if (type.IsAssignableFrom( updated.GetType() ) == false)
            {
                throw new Exception( "Cannot reconfigure a " + updated.GetType().ToString() + " into a " + type.ToString() );
            }

            EmulatorComponent original = updated.Clone();

            updated.Configure( config );

            if (updated.IsReplaceableBy( original ) == false)
            {
                // the crucial ID info has changed
                throw new Exception( "The identification information of " + updated.GetType().ToString() + " cannot be updated in " + updatedId + "." );
            }

            if (componentId != null)
            {
                updated.ComponentId = componentId;
            }

            return updated;
        }

        private void RemoveEmulatorComponent( Type type, String removedId )
        {
            EmulatorComponent removed = _emulator.FindComponentById( removedId );

            if (removed == null)
            {
                throw new Exception( "Cannot find component " + removedId + "." );
            }

            if (type.IsAssignableFrom( removed.GetType() ) == false)
            {
                throw new Exception( "Type mismatched -- " + removed.GetType().ToString() + " and " + type.ToString() + "." );
            }

            _emulator.UnregisterComponent( removed );
        }

        // default implementation for EmulatorComponent.Configure()
        public void ConfigureEmulatorComponent( EmulatorComponent component, XmlReader config )
        {
            Type objType = component.GetType();
            config.ReadStartElement(); // bypass the root element

            while (true)
            {
                config.Read();

                if (config.IsStartElement())
                {
                    String propertyName = config.Name;
                    PropertyInfo property = objType.GetProperty( propertyName );

                    if (property == null)
                    {
                        throw new XmlException( "Unrecognize element (Property): " + propertyName );
                    }

                    if (property.CanWrite == false)
                    {
                        throw new Exception( "The element (Property), " + propertyName + ", is read-only." );
                    }

                    Type propertyType = property.PropertyType;
                    Type actualType = propertyType;

                    if (config.MoveToAttribute( "type" ))
                    {
                        actualType = ResolveType( config.Value );
                    }

                    config.MoveToElement();

                    if (typeof( EmulatorComponent ).IsAssignableFrom( propertyType ) ||
                        typeof( EmulatorComponent ).IsAssignableFrom( actualType ))
                    {
                        XmlReader ecConfig = config.ReadSubtree();

                        EmulatorComponent ec = ProcessEmulatorComponent( actualType, ecConfig );

                        ecConfig.Close();

                        if (propertyType.IsAssignableFrom( ec.GetType() ) == false)
                        {
                            throw new Exception( "Type mismatch: " + ec.GetType().ToString() + " is not a " + propertyType.ToString() );
                        }

                        if (ec.LinkedBy == null)
                        {
                            ec.LinkedBy = component;
                        }
                        else if (ec.LinkedBy != component)
                        {
                            throw new Exception( "This EmulatorComponent is already in " + ec.LinkedBy.ComponentId + "." );
                        }

                        if (component.LinkedComponents.Contains( ec ) == false)
                        {
                            component.LinkedComponents.Add( ec );
                        }

                        EmulatorComponent oldEc = (EmulatorComponent)property.GetValue( component, null );
                        if ((oldEc != null) && (oldEc != ec))
                        {
                            _emulator.UnregisterComponent( oldEc );
                        }

                        property.SetValue( component, ec, null );
                    }
                    else
                    {
                        XmlReader objectConfig = config.ReadSubtree();
                        property.SetValue( component, ParseObject( propertyType, objectConfig ), null );
                        objectConfig.Close();
                    }
                }
                else
                {
                    break;
                }
            }
        }

        [Flags]
        private enum ParseObjectFlags
        {
            None = 0,
            Type = 1,
            Format = 2,
            Length = 4,
            HasChildren = 8,
            IsArray = 16,
        }

        private Object ParseObject( Type type, XmlReader config )
        {
            Object result = null;
            ParseObjectFlags flags = ParseObjectFlags.None;
            String formatString = "", elementString = "";
            Type objType = type;
            int arrayLength = 0;

            config.MoveToContent();

            if (config.MoveToAttribute( "type" ))
            {
                objType = ResolveType( config.Value );
                flags |= ParseObjectFlags.Type;
            }
            if (config.MoveToAttribute( "format" ))
            {
                formatString = config.Value;
                flags |= ParseObjectFlags.Format;
            }
            if (config.MoveToAttribute( "length" ))
            {
                arrayLength = Int32.Parse( config.Value );
                flags |= ParseObjectFlags.Length;
            }

            config.MoveToElement();

            // read the next node
            config.Read();

            if (config.IsStartElement() || config.NodeType == XmlNodeType.EndElement || config.NodeType == XmlNodeType.None)
            {
                flags |= ParseObjectFlags.HasChildren;
                // the XmlReader is pointing at the first child element
            }
            else
            {
                elementString = config.ReadContentAsString();

                if (elementString.Length == 0)
                {
                    // if the content is empty, we consider it as having 0 children
                    flags |= ParseObjectFlags.HasChildren;
                }
                // the XmlReader is pointing at the next element
            }

            if (objType.IsArray == true)
            {
                flags |= ParseObjectFlags.IsArray;
            }

            switch (flags)
            {
                case ParseObjectFlags.None:
                case ParseObjectFlags.Type:
                    {
                        if (objType.IsEnum)
                        {
                            result = Enum.Parse( objType, elementString );
                        }
                        else if (objType == typeof( String ))
                        {
                            result = elementString;
                        }
                        else
                        {
                            Type[] stringParam = { typeof( String ) };
                            MethodInfo parseMethod = objType.GetMethod( "Parse", stringParam );
                            if (parseMethod == null || parseMethod.IsStatic == false)
                            {
                                throw new Exception( "Can't find the Parse(String) method in " + objType.ToString() );
                            }

                            Object[] parseParam = { elementString };

                            result = parseMethod.Invoke( null, parseParam );
                        }
                    }
                    break;
                case ParseObjectFlags.Format:
                case ParseObjectFlags.Format | ParseObjectFlags.Type:
                    {
                        MethodInfo[] allPublicStatic = objType.GetMethods( BindingFlags.Static | BindingFlags.Public );

                        foreach (MethodInfo m in allPublicStatic)
                        {
                            ParameterInfo[] parameters = m.GetParameters();

                            if (m.Name == "Parse" &&
                                parameters.Length == 2 &&
                                parameters[0].ParameterType == typeof( String ) &&
                                parameters[1].ParameterType.IsEnum == true)
                            {
                                Object formatStyle;

                                try
                                {
                                    formatStyle = Enum.Parse( parameters[1].ParameterType, formatString, true );
                                }
                                catch (ArgumentException)
                                {
                                    continue;
                                }

                                Object[] parseParam = { elementString, formatStyle };

                                result = m.Invoke( null, parseParam );
                                break;
                            }
                        }

                        if (result == null)
                        {
                            throw new Exception( "Can't find the appropriate Parse method to take the format in " + objType.ToString() );
                        }
                    }
                    break;
                case ParseObjectFlags.HasChildren:
                case ParseObjectFlags.HasChildren | ParseObjectFlags.Type:
                    {
                        result = Activator.CreateInstance( objType );

                        // The XmlReader is pointing at the first parameter element 
                        while (true)
                        {
                            if (config.IsStartElement())
                            {
                                PropertyInfo property = objType.GetProperty( config.Name );
                                FieldInfo field = objType.GetField( config.Name );
                                Type pfType;

                                if (property != null)
                                {
                                    pfType = property.PropertyType;
                                }
                                else if (field != null)
                                {
                                    pfType = field.FieldType;
                                }
                                else
                                {
                                    throw new Exception( "Cannot find property " + config.Name + " in type " + objType.ToString() + "." );
                                }

                                if (config.MoveToAttribute( "type" ))
                                {
                                    pfType = ResolveType( config.Value );
                                }

                                config.MoveToElement();

                                XmlReader objConfig = config.ReadSubtree();

                                Object obj = ParseObject( pfType, objConfig );

                                if (property != null)
                                {
                                    property.SetValue( result, obj, null );
                                }
                                else
                                {
                                    field.SetValue( result, obj );
                                }

                                objConfig.Close();
                            }
                            else
                            {
                                break;
                            }

                            config.Read();
                        }
                    }
                    break;
                case ParseObjectFlags.IsArray | ParseObjectFlags.HasChildren:
                case ParseObjectFlags.IsArray | ParseObjectFlags.HasChildren | ParseObjectFlags.Length:
                    {
                        Type listType = typeof( List<> ).MakeGenericType( objType.GetElementType() );
                        Object list = Activator.CreateInstance( listType );
                        MethodInfo addMethod = listType.GetMethod( "Add" );

                        // The XmlReader is pointing at the first parameter element 
                        while (true)
                        {
                            if (config.IsStartElement())
                            {
                                Type elementType = ResolveType( config.Name );

                                XmlReader elementConfig = config.ReadSubtree();

                                Object[] parameters = { ParseObject( elementType, elementConfig ) };

                                addMethod.Invoke( list, parameters );

                                elementConfig.Close();
                            }
                            else
                            {
                                break;
                            }

                            config.Read();
                        }

                        Array resultArrayRaw = (Array)listType.GetMethod( "ToArray" ).Invoke( list, null );
                        result = resultArrayRaw;

                        if ((flags & ParseObjectFlags.Length) != ParseObjectFlags.None)
                        {
                            int resultLen = resultArrayRaw.Length;
                            if (resultLen < arrayLength)
                            {
                                result = Array.CreateInstance( objType.GetElementType(), arrayLength );

                                Array.Copy( resultArrayRaw, (Array)result, resultLen );
                            }
                            else if (resultLen > arrayLength)
                            {
                                throw new Exception( "The array exceeds the specified capacity of " + arrayLength );
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception( "Invalid syntax in parsing object of type " + objType.ToString() + "." );
            }

            return result;
        }

        private Type ResolveEmulatorComponentType( String typeName )
        {
            Type objType = ResolveType( typeName );

            if (objType.IsSubclassOf( typeof( EmulatorComponent ) ) == false)
            {
                throw new Exception( typeName + " is not an EmulatorComponent." );
            }

            return objType;
        }

        private Type ResolveType( String typeName )
        {
            Type objType;

            if ((objType = Type.GetType( typeName, false )) != null)
            {
                return objType;
            }

            if ((objType = _defaultAssembly.GetType( typeName, false )) != null)
            {
                return objType;
            }

            if (_typesLookup.ContainsKey( typeName ))
            {
                String fullName = _typesLookup[typeName];

                if ((objType = Type.GetType( fullName, false )) != null)
                {
                    return objType;
                }

                if ((objType = _defaultAssembly.GetType( fullName, false )) != null)
                {
                    return objType;
                }

                Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();

                for (int i = 0; i < asms.Length; i++)
                {
                    if ((objType = asms[i].GetType(fullName, false)) != null)
                    {
                        return objType;
                    }
                }                
            }

            throw new Exception( "Cannot resolve type " + typeName );
        }
    }
}