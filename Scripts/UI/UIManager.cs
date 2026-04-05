using Godot;

public partial class UIManager : Control
{
    [Export]
    public NodePath PlanetSizeOptionButtonPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/PlanetSizeRow/PlanetSizeOptionButton";

    [Export]
    public NodePath LandmassModeOptionButtonPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/LandmassModeRow/LandmassModeOptionButton";

    [Export]
    public NodePath WaterSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/WaterRow/WaterSlider";

    [Export]
    public NodePath WaterValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/WaterRow/WaterValueLabel";

    [Export]
    public NodePath WetlandsSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/WetlandsRow/WetlandsSlider";

    [Export]
    public NodePath WetlandsValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/WetlandsRow/WetlandsValueLabel";

    [Export]
    public NodePath PlainsSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/PlainsRow/PlainsSlider";

    [Export]
    public NodePath PlainsValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/PlainsRow/PlainsValueLabel";

    [Export]
    public NodePath ForestSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/ForestRow/ForestSlider";

    [Export]
    public NodePath ForestValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/ForestRow/ForestValueLabel";

    [Export]
    public NodePath DesertSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/DesertRow/DesertSlider";

    [Export]
    public NodePath DesertValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/DesertRow/DesertValueLabel";

    [Export]
    public NodePath MountainsSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/MountainsRow/MountainsSlider";

    [Export]
    public NodePath MountainsValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/MountainsRow/MountainsValueLabel";

    [Export]
    public NodePath NoiseScaleSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/NoiseScaleRow/NoiseScaleSlider";

    [Export]
    public NodePath NoiseScaleValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/NoiseScaleRow/NoiseScaleValueLabel";

    [Export]
    public NodePath MountainNoiseScaleSliderPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/MountainNoiseScaleRow/MountainNoiseScaleSlider";

    [Export]
    public NodePath MountainNoiseScaleValueLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/MountainNoiseScaleRow/MountainNoiseScaleValueLabel";

    [Export]
    public NodePath LandPercentLabelPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/LandSummaryRow/LandPercentValue";

    [Export]
    public NodePath GenerateButtonPath { get; set; } = "RootMargin/MainSplit/SettingsPanel/SettingsMargin/SettingsVBox/GeneratePlanetButton";

    [Export]
    public NodePath GeneratorPath { get; set; } = "RootMargin/MainSplit/PreviewPanel/PreviewMargin/PreviewViewportContainer/PreviewViewport/PreviewRoot/PlanetPivot/PlanetGenerator";

    [Export]
    public NodePath PreviewControllerPath { get; set; } = "RootMargin/MainSplit/PreviewPanel/PreviewMargin/PreviewViewportContainer/PreviewViewport/PreviewRoot";

    [Export]
    public PlanetSettings DefaultSettings { get; set; }

    private OptionButton _planetSizeOptionButton = null;
    private OptionButton _landmassModeOptionButton = null;
    private HSlider _waterSlider = null;
    private Label _waterValueLabel = null;
    private HSlider _wetlandsSlider = null;
    private Label _wetlandsValueLabel = null;
    private HSlider _plainsSlider = null;
    private Label _plainsValueLabel = null;
    private HSlider _forestSlider = null;
    private Label _forestValueLabel = null;
    private HSlider _desertSlider = null;
    private Label _desertValueLabel = null;
    private HSlider _mountainsSlider = null;
    private Label _mountainsValueLabel = null;
    private HSlider _noiseScaleSlider = null;
    private Label _noiseScaleValueLabel = null;
    private HSlider _mountainNoiseScaleSlider = null;
    private Label _mountainNoiseScaleValueLabel = null;
    private Label _landPercentLabel = null;
    private Button _generateButton = null;
    private PlanetGenerator _generator = null;
    private PlanetPreviewController _previewController = null;

    public override void _Ready()
    {
        ResolveNodes();
        ConfigureOptions();
        BindSignals();
        ApplySettingsToUi(DefaultSettings ?? new PlanetSettings());
        GeneratePlanet();
    }

    private void ResolveNodes()
    {
        _planetSizeOptionButton = GetNode<OptionButton>(PlanetSizeOptionButtonPath);
        _landmassModeOptionButton = GetNode<OptionButton>(LandmassModeOptionButtonPath);
        _waterSlider = GetNode<HSlider>(WaterSliderPath);
        _waterValueLabel = GetNode<Label>(WaterValueLabelPath);
        _wetlandsSlider = GetNode<HSlider>(WetlandsSliderPath);
        _wetlandsValueLabel = GetNode<Label>(WetlandsValueLabelPath);
        _plainsSlider = GetNode<HSlider>(PlainsSliderPath);
        _plainsValueLabel = GetNode<Label>(PlainsValueLabelPath);
        _forestSlider = GetNode<HSlider>(ForestSliderPath);
        _forestValueLabel = GetNode<Label>(ForestValueLabelPath);
        _desertSlider = GetNode<HSlider>(DesertSliderPath);
        _desertValueLabel = GetNode<Label>(DesertValueLabelPath);
        _mountainsSlider = GetNode<HSlider>(MountainsSliderPath);
        _mountainsValueLabel = GetNode<Label>(MountainsValueLabelPath);
        _noiseScaleSlider = GetNode<HSlider>(NoiseScaleSliderPath);
        _noiseScaleValueLabel = GetNode<Label>(NoiseScaleValueLabelPath);
        _mountainNoiseScaleSlider = GetNode<HSlider>(MountainNoiseScaleSliderPath);
        _mountainNoiseScaleValueLabel = GetNode<Label>(MountainNoiseScaleValueLabelPath);
        _landPercentLabel = GetNode<Label>(LandPercentLabelPath);
        _generateButton = GetNode<Button>(GenerateButtonPath);
        _generator = GetNode<PlanetGenerator>(GeneratorPath);
        _previewController = GetNode<PlanetPreviewController>(PreviewControllerPath);
    }

    private void ConfigureOptions()
    {
        _planetSizeOptionButton.Clear();
        _planetSizeOptionButton.AddItem("Small", (int)PlanetSizeOption.Small);
        _planetSizeOptionButton.AddItem("Medium", (int)PlanetSizeOption.Medium);
        _planetSizeOptionButton.AddItem("Large", (int)PlanetSizeOption.Large);

        _landmassModeOptionButton.Clear();
        _landmassModeOptionButton.AddItem("Large Continents", (int)PlanetLandmassMode.LargeContinents);
        _landmassModeOptionButton.AddItem("Medium Continents", (int)PlanetLandmassMode.MediumContinents);
        _landmassModeOptionButton.AddItem("Archipelago", (int)PlanetLandmassMode.Archipelago);
    }

    private void BindSignals()
    {
        _planetSizeOptionButton.ItemSelected += OnPlanetSizeSelected;
        _landmassModeOptionButton.ItemSelected += OnLandmassModeSelected;
        _waterSlider.ValueChanged += OnSliderValueChanged;
        _wetlandsSlider.ValueChanged += OnSliderValueChanged;
        _plainsSlider.ValueChanged += OnSliderValueChanged;
        _forestSlider.ValueChanged += OnSliderValueChanged;
        _desertSlider.ValueChanged += OnSliderValueChanged;
        _mountainsSlider.ValueChanged += OnSliderValueChanged;
        _noiseScaleSlider.ValueChanged += OnSliderValueChanged;
        _mountainNoiseScaleSlider.ValueChanged += OnSliderValueChanged;
        _generateButton.Pressed += OnGeneratePlanetPressed;
    }

    private void ApplySettingsToUi(PlanetSettings settings)
    {
        SelectOptionById(_planetSizeOptionButton, (int)settings.PlanetSize);
        SelectOptionById(_landmassModeOptionButton, (int)settings.LandmassMode);
        _waterSlider.Value = settings.WaterPercent;
        _wetlandsSlider.Value = settings.WetlandsPercent;
        _plainsSlider.Value = settings.PlainsPercent;
        _forestSlider.Value = settings.ForestPercent;
        _desertSlider.Value = settings.DesertPercent;
        _mountainsSlider.Value = settings.MountainPercent;
        _noiseScaleSlider.Value = settings.NoiseScale;
        _mountainNoiseScaleSlider.Value = settings.MountainNoiseScale;
        RefreshValueLabels();
    }

    private static void SelectOptionById(OptionButton optionButton, int id)
    {
        for (int i = 0; i < optionButton.ItemCount; i++)
        {
            if (optionButton.GetItemId(i) == id)
            {
                optionButton.Select(i);
                return;
            }
        }

        optionButton.Select(0);
    }

    private void OnPlanetSizeSelected(long index)
    {
        RefreshValueLabels();
    }

    private void OnLandmassModeSelected(long index)
    {
        RefreshValueLabels();
    }

    private void OnSliderValueChanged(double value)
    {
        RefreshValueLabels();
    }

    private void OnGeneratePlanetPressed()
    {
        GeneratePlanet();
    }

    private void GeneratePlanet()
    {
        PlanetSettings settings = BuildSettingsFromUi();
        _generator.Generate(settings);
        _previewController.FramePlanet(settings);
    }

    private PlanetSettings BuildSettingsFromUi()
    {
        PlanetSettings settings = DefaultSettings != null ? (PlanetSettings)DefaultSettings.Duplicate() : new PlanetSettings();

        settings.PlanetSize = (PlanetSizeOption)_planetSizeOptionButton.GetSelectedId();
        settings.LandmassMode = (PlanetLandmassMode)_landmassModeOptionButton.GetSelectedId();
        settings.WaterPercent = (float)_waterSlider.Value;
        settings.WetlandsPercent = (float)_wetlandsSlider.Value;
        settings.PlainsPercent = (float)_plainsSlider.Value;
        settings.ForestPercent = (float)_forestSlider.Value;
        settings.DesertPercent = (float)_desertSlider.Value;
        settings.MountainPercent = (float)_mountainsSlider.Value;
        settings.NoiseScale = (float)_noiseScaleSlider.Value;
        settings.MountainNoiseScale = (float)_mountainNoiseScaleSlider.Value;

        return settings;
    }

    private void RefreshValueLabels()
    {
        _waterValueLabel.Text = $"{_waterSlider.Value:0.#}%";
        _wetlandsValueLabel.Text = $"{_wetlandsSlider.Value:0.#}%";
        _plainsValueLabel.Text = $"{_plainsSlider.Value:0.#}%";
        _forestValueLabel.Text = $"{_forestSlider.Value:0.#}%";
        _desertValueLabel.Text = $"{_desertSlider.Value:0.#}%";
        _mountainsValueLabel.Text = $"{_mountainsSlider.Value:0.#}%";
        _noiseScaleValueLabel.Text = $"{_noiseScaleSlider.Value:0.000}";
        _mountainNoiseScaleValueLabel.Text = $"{_mountainNoiseScaleSlider.Value:0.000}";

        double landPercent = _wetlandsSlider.Value + _plainsSlider.Value + _forestSlider.Value + _desertSlider.Value + _mountainsSlider.Value;
        _landPercentLabel.Text = $"{landPercent:0.#}%";
    }
}
