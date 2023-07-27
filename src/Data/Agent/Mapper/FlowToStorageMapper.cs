using System.Globalization;
using AutoMapper;
using AyBorg.SDK.Common;
using AyBorg.SDK.Common.Ports;
using AyBorg.SDK.Projects;
using ImageTorque;

namespace AyBorg.Data.Agent;

public sealed class FlowToStorageMapper : IFlowToStorageMapper
{
    private readonly Mapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlowToStorageMapper"/> class.
    /// </summary>
    public FlowToStorageMapper()
    {
        var config = new MapperConfiguration(config =>
        {
            config.CreateMap<ProjectMeta, ProjectMetaRecord>().ReverseMap();
            config.CreateMap<ProjectSettings, ProjectSettingsRecord>().ReverseMap();
            config.CreateMap<Project, ProjectRecord>();
            config.CreateMap<PluginMetaInfo, PluginMetaInfoRecord>().ReverseMap();
            config.CreateMap<IStepProxy, StepRecord>();
            config.CreateMap<PortLink, LinkRecord>();

            // Ports
            config.CreateMap<NumericPort, StepPortRecord>().ForMember(d => d.Value, opt => opt.MapFrom(s => Convert.ToString(s.Value, CultureInfo.InvariantCulture)));
            config.CreateMap<StringPort, StepPortRecord>();
            config.CreateMap<FolderPort, StepPortRecord>();
            config.CreateMap<BooleanPort, StepPortRecord>();
            config.CreateMap<ImagePort, StepPortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new ImageToRecordConverter()));
            config.CreateMap<RectanglePort, StepPortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new RectangleToRecordConverter()));
            config.CreateMap<EnumPort, StepPortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new EnumToRecordConverter()));
            config.CreateMap<SelectPort, StepPortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new SelectValueToRecordConverter()));
            // Port collections
            config.CreateMap<StringCollectionPort, StepPortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<string>()));
            config.CreateMap<NumericCollectionPort, StepPortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<double>()));
            config.CreateMap<RectangleCollectionPort, StepPortRecord>().ForMember(d => d.Value, opt => opt.ConvertUsing(new CollectionToRecordConverter<Rectangle>()));
        });

        _mapper = new Mapper(config);
    }

    /// <summary>
    /// Maps the specified port.
    /// </summary>
    /// <param name="port">The port.</param>
    /// <returns></returns>
    public StepPortRecord Map(IPort port)
    {
        return _mapper.Map<StepPortRecord>(port);
    }

    /// <summary>
    /// Maps the specified step proxy.
    /// </summary>
    /// <param name="stepProxy">The step proxy.</param>
    /// <returns></returns>
    public StepRecord Map(IStepProxy stepProxy)
    {
        return _mapper.Map<StepRecord>(stepProxy);
    }

    /// <summary>
    /// Maps the specified project.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns></returns>
    public ProjectRecord Map(Project project)
    {
        return _mapper.Map<ProjectRecord>(project);
    }
}
