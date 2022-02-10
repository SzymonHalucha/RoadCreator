using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using SH.RoadCreator.Algorithm;

namespace SH.RoadCreator.Viusal
{
    /// <summary>
    /// This class is responsible for displaying the window of a specific segment project.
    /// </summary>
    public class SelectedSegment
    {
        private VisualElement _root;
        private SegmentEditorWindow _window;
        private Segment _current;
        private SegmentDisplay _display;
        private Item _active;

        private readonly string[] _leftKeywords = new string[] { "0", "a", "left", "l" };
        private readonly string[] _rightKeywords = new string[] { "1", "b", "right", "r" };

        private const string uss_container = "container";
        private const string uss_section = "section";
        private const string uss_sectionTitle = "section-title";
        private const string uss_sectionFoldout = "section-foldout";
        private const string uss_sectionFoldoutItem = "section-foldout-item";
        private const string uss_sectionButtonsContainer = "section-buttons-container";
        private const string uss_sectionButton = "section-button";
        private const string uss_active = "active";
        private const string uss_hide = "hide";

        public enum Section { Settings, Modules, Patterns }

        public class Item
        {
            public Foldout Field { get; set; }
            public Section Type { get; set; }
            public int Index { get; set; }

            public Item(Foldout field, Section type, int index)
            {
                this.Field = field;
                this.Type = type;
                this.Index = index;
            }
        }

        public SelectedSegment(VisualElement root, SegmentEditorWindow window)
        {
            this._root = root;
            this._window = window;
        }

        /// <summary>
        /// Update the window view to a specific segment project.
        /// </summary>
        /// <param name="segment">Selected segment project.</param>
        /// <param name="display">An object responsible for the logic of displaying the segment mesh and texture in the editor scene.</param>
        public void UpdateGUI(Segment segment, SegmentDisplay display)
        {
            _current = segment;
            _display = display;

            VisualElement settings = _root.Q<VisualElement>("section-settings");
            settings.Clear();
            CreateSettingsSection(settings);

            VisualElement modules = _root.Q<VisualElement>("section-modules");
            modules.Clear();
            CreateModulesSection(modules);
            CreateButtonsForSection(modules, Section.Modules);

            VisualElement patterns = _root.Q<VisualElement>("section-patterns");
            patterns.Clear();
            CreatePatternsSection(patterns);
            CreateButtonsForSection(patterns, Section.Patterns);
        }

        /// <summary>
        /// Create section content for general settings in the segment editor window.
        /// </summary>
        /// <param name="section">Selected section object.</param>
        private void CreateSettingsSection(VisualElement section)
        {
            CreateSectionTitle(section, _current.name);

            Foldout general = CreateFoldout(section, "General Settings");
            Item item = new Item(general, Section.Settings, 0);
            general.RegisterCallback<ClickEvent, Item>(OnFoldoutClicked, item);

            SerializedObject serializedSegment = new SerializedObject(_current);
            CreatePropertyField(general, serializedSegment.FindProperty("Length"));
            CreatePropertyField(general, serializedSegment.FindProperty("Resolution"));

            Button generate = CreateButton(general, uss_sectionFoldoutItem, "Generate Preview");
            generate.clicked += () => { _display.GenerateSegmentMesh(_current); };

            Button clear = CreateButton(general, uss_sectionFoldoutItem, "Clear Preview");
            clear.clicked += () => { _display.ClearSegmentMesh(); };

            Button list = CreateButton(general, uss_sectionFoldoutItem, "View Segment List");
            list.clicked += () => { _window.UpdateGUI(); };
        }

        /// <summary>
        /// Create content section for the segment module list.
        /// </summary>
        /// <param name="section">Selected section object.</param>
        private void CreateModulesSection(VisualElement section)
        {
            CreateSectionTitle(section, "Modules");

            SerializedObject serializedSegment = new SerializedObject(_current);
            SerializedProperty serializedModules = serializedSegment.FindProperty("Modules");

            for (int i = 0; i < serializedModules.arraySize; i++)
            {
                SerializedProperty module = serializedModules.GetArrayElementAtIndex(i);
                SerializedProperty name = module.FindPropertyRelative("Name");

                Foldout foldout = CreateFoldout(section, name.stringValue);
                Item item = new Item(foldout, Section.Modules, i);
                foldout.RegisterCallback<ClickEvent, Item>(OnFoldoutClicked, item);

                CreatePropertyField(foldout, module, nameof(Module.Name)).RegisterCallback<InputEvent, Item>(OnNameFieldChanged, item);
                CreatePropertyField(foldout, module, nameof(Module.Width));
                CreatePropertyField(foldout, module, nameof(Module.Height));
                CreatePropertyField(foldout, module, nameof(Module.BaseColor));
            }
        }

        /// <summary>
        /// Create content section for the segment pattern list.
        /// </summary>
        /// <param name="section">Selected section object.</param>
        private void CreatePatternsSection(VisualElement section)
        {
            CreateSectionTitle(section, "Patterns");

            SerializedObject serializedSegment = new SerializedObject(_current);
            SerializedProperty serializedPatterns = serializedSegment.FindProperty("Patterns");

            for (int i = 0; i < serializedPatterns.arraySize; i++)
            {
                SerializedProperty pattern = serializedPatterns.GetArrayElementAtIndex(i);
                SerializedProperty name = pattern.FindPropertyRelative("Name");

                Foldout foldout = CreateFoldout(section, name.stringValue);
                Item item = new Item(foldout, Section.Patterns, i);
                foldout.RegisterCallback<ClickEvent, Item>(OnFoldoutClicked, item);

                CreatePropertyField(foldout, pattern, nameof(Pattern.Name)).RegisterCallback<InputEvent, Item>(OnNameFieldChanged, item);
                CreatePropertyField(foldout, pattern, nameof(Pattern.Size));
                CreatePropertyField(foldout, pattern, nameof(Pattern.Tiling));
                CreatePropertyField(foldout, pattern, nameof(Pattern.Offset));
                CreatePropertyField(foldout, pattern, nameof(Pattern.SymmetryX));
                CreatePropertyField(foldout, pattern, nameof(Pattern.SymmetryY));
                CreatePropertyField(foldout, pattern, nameof(Pattern.BaseColor));
                CreatePropertyField(foldout, pattern, nameof(Pattern.PatternType));
            }
        }

        /// <summary>
        /// Create a title header for the section.
        /// </summary>
        /// <param name="parent">Parent of the title header.</param>
        /// <param name="name">Text of the title.</param>
        private void CreateSectionTitle(VisualElement parent, string name)
        {
            Label title = new Label(name);
            title.AddToClassList(uss_sectionTitle);
            parent.Add(title);
        }

        /// <summary>
        /// Utwórz przyciski manipulacji listą modułów lub wzorców dla wybranej sekcji.
        /// Create buttons to manipulate the list of modules or patterns for the selected section.
        /// </summary>
        /// <param name="section">Parent of the buttons.</param>
        /// <param name="type">Selected section type (Modules or Patterns only).</param>
        private void CreateButtonsForSection(VisualElement section, Section type)
        {
            if (Section.Settings == type) return;

            VisualElement container = new VisualElement();
            container.AddToClassList(uss_sectionButtonsContainer);
            section.Add(container);

            Button add = CreateButton(container, uss_sectionButton, "Add");
            add.clicked += () => { OnAddButtonClicked(type); };

            Button remove = CreateButton(container, uss_sectionButton, "Remove");
            remove.clicked += () => { OnRemoveButtonClicked(type); };

            Button copy = CreateButton(container, uss_sectionButton, "Copy");
            copy.clicked += () => { OnCopyButtonClicked(type); };

            if (Section.Patterns == type) return;

            Button mirror = CreateButton(container, uss_sectionButton, "Mirror");
            mirror.clicked += () => { OnMirrorButtonClicked(type); };
        }

        private void OnFoldoutClicked(ClickEvent e, Item item)
        {
            if (_active != null)
            {
                _active.Field.RemoveFromClassList(uss_active);
                _active.Field.SetValueWithoutNotify(false);
            }

            _active = item;
            if (_active == null) return;

            _active.Field.AddToClassList(uss_active);
            _active.Field.SetValueWithoutNotify(true);
        }

        private void OnNameFieldChanged(InputEvent e, Item item)
        {
            item.Field.text = e.newData;
        }

        private void OnAddButtonClicked(Section type)
        {
            if (Section.Modules == type) _current.Modules.Add(new Module());
            if (Section.Patterns == type) _current.Patterns.Add(new Pattern());

            UpdateGUI(_current, _display);
        }

        private void OnRemoveButtonClicked(Section type)
        {
            if (Section.Modules == type && _current.Modules.Count <= 0) return;
            if (Section.Patterns == type && _current.Patterns.Count <= 0) return;

            if (Section.Modules == type && (_active == null || Section.Modules != _active.Type))
                _current.Modules.RemoveAt(_current.Modules.Count - 1);

            else if (Section.Patterns == type && (_active == null || Section.Patterns != _active.Type))
                _current.Patterns.RemoveAt(_current.Patterns.Count - 1);

            else if (Section.Modules == type && _active.Type == type)
                _current.Modules.RemoveAt(_active.Index);

            else if (Section.Patterns == type && _active.Type == type)
                _current.Patterns.RemoveAt(_active.Index);

            _active = null;
            UpdateGUI(_current, _display);
        }

        private void OnCopyButtonClicked(Section type)
        {
            if (_active == null) return;
            if (Section.Modules == type && Section.Modules != _active.Type) return;
            if (Section.Patterns == type && Section.Patterns != _active.Type) return;

            if (Section.Modules == type)
            {
                Module toCopy = _current.Modules[_active.Index];
                _current.Modules.Add(new Module(toCopy));
            }
            else if (Section.Patterns == type)
            {
                Pattern toCopy = _current.Patterns[_active.Index];
                _current.Patterns.Add(new Pattern(toCopy));
            }

            UpdateGUI(_current, _display);
        }

        private void OnMirrorButtonClicked(Section type)
        {
            List<Module> left = new List<Module>();
            List<Module> right = new List<Module>();

            foreach (Module module in _current.Modules)
                if (HaveKeywords(_leftKeywords, module.Name.ToLower()))
                    left.Add(module);

            foreach (Module module in _current.Modules)
                if (HaveKeywords(_rightKeywords, module.Name.ToLower()))
                    right.Add(module);

            for (int i = left.Count - 1; i >= 0; i--)
            {
                Module mirror = new Module(left[i]);
                mirror.Name = ReplaceKeywordToOpposite(_leftKeywords, _rightKeywords, mirror.Name.ToLower());
                _current.Modules.Add(mirror);
            }

            for (int i = right.Count - 1; i >= 0; i--)
            {
                Module mirror = new Module(right[i]);
                mirror.Name = ReplaceKeywordToOpposite(_rightKeywords, _leftKeywords, mirror.Name.ToLower());
                _current.Modules.Add(mirror);
            }

            UpdateGUI(_current, _display);
        }

        private bool HaveKeywords(string[] keywords, string word)
        {
            foreach (string keyword in keywords)
                if (word.Contains("_" + keyword))
                    return true;

            return false;
        }

        private string ReplaceKeywordToOpposite(string[] currentKeywords, string[] oppositeKeywords, string word)
        {
            for (int i = 0; i < currentKeywords.Length; i++)
                if (word.Contains("_" + currentKeywords[i]))
                    word = word.Replace("_" + currentKeywords[i], "_" + oppositeKeywords[i]);

            return word;
        }

        /// <summary>
        /// Create a single foldout field with the default USS class.
        /// </summary>
        /// <returns>Returns the created field.</returns>
        private Foldout CreateFoldout()
        {
            Foldout foldout = new Foldout();
            foldout.AddToClassList(uss_sectionFoldout);
            foldout.value = false;

            return foldout;
        }

        /// <summary>
        /// Create a single foldout field with the default USS class.
        /// </summary>
        /// <param name="parent">Parent of the created field.</param>
        /// <param name="label">Name of the created field.</param>
        /// <returns>Returns the created field.</returns>
        private Foldout CreateFoldout(VisualElement parent, string label)
        {
            Foldout foldout = new Foldout();
            foldout.AddToClassList(uss_sectionFoldout);
            foldout.value = false;
            foldout.text = label;
            parent.Add(foldout);

            return foldout;
        }

        /// <summary>
        /// Create a single field for a floating-point number type.
        /// </summary>
        /// <param name="parent">Parent of the created field.</param>
        /// <param name="value">Initial value.</param>
        /// <param name="label">Name of the created field.</param>
        /// <returns>Returns the created field.</returns>
        private FloatField CreateFloatField(VisualElement parent, float value, string label)
        {
            FloatField field = new FloatField();
            field.AddToClassList(uss_sectionFoldoutItem);
            field.value = value;
            field.label = label;
            parent.Add(field);

            return field;
        }

        /// <summary>
        /// Create a single toggle field.
        /// </summary>
        /// <param name="parent">Parent of the created field.</param>
        /// <param name="value">Initial value.</param>
        /// <param name="label">Name of the created field.</param>
        /// <returns>Returns the created field.</returns>
        private Toggle CreateToggle(VisualElement parent, bool value, string label)
        {
            Toggle toggle = new Toggle();
            toggle.AddToClassList(uss_sectionFoldoutItem);
            toggle.value = value;
            toggle.label = label;
            parent.Add(toggle);

            return toggle;
        }

        /// <summary>
        /// Create a single default field for the seralized variable regardless of type.
        /// </summary>
        /// <param name="parent">Parent of the created field.</param>
        /// <param name="property">Serialized variable.</param>
        /// <returns>Returns the created field.</returns>
        private PropertyField CreatePropertyField(VisualElement parent, SerializedProperty property)
        {
            PropertyField field = new PropertyField();
            field.AddToClassList(uss_sectionFoldoutItem);
            field.BindProperty(property);
            parent.Add(field);

            return field;
        }

        /// <summary>
        /// Create a single default field for the variable with a specific name regardless of type from the serialized property.
        /// </summary>
        /// <param name="parent">Parent of the created field.</param>
        /// <param name="property">Serialized property.</param>
        /// <param name="name">Selected variable name.</param>
        /// <returns>Returns the created field.</returns>
        private PropertyField CreatePropertyField(VisualElement parent, SerializedProperty property, string name)
        {
            PropertyField field = new PropertyField();
            field.AddToClassList(uss_sectionFoldoutItem);
            field.BindProperty(property.FindPropertyRelative(name));
            parent.Add(field);

            return field;
        }

        /// <summary>
        /// Create a single button.
        /// </summary>
        /// <param name="parent">Parent of the structure.</param>
        /// <param name="ussName">USS class name to assign.</param>
        /// <param name="text">Text displayed on the button.</param>
        /// <returns>Returns the created button.</returns>
        private Button CreateButton(VisualElement parent, string ussName, string text)
        {
            Button button = new Button();
            button.AddToClassList(ussName);
            button.text = text;
            parent.Add(button);

            return button;
        }
    }
}