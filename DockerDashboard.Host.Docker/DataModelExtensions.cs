using Docker.DotNet.Models;
using DockerDashboard.Shared.Data;
using MountPoint = Docker.DotNet.Models.MountPoint;

namespace DockerDashboard.Host.Docker;

internal static class DataModelExtensions
{
    public static Shared.Data.ContainerStatus MapStatus(string status) => status.ToLower() switch
    {
        { } d when d.StartsWith("exited") => Shared.Data.ContainerStatus.Exited,
        { } d when d.StartsWith("created") => Shared.Data.ContainerStatus.Created,
        { } d when d.StartsWith("restarting") => Shared.Data.ContainerStatus.Restarted,
        { } d when d.StartsWith("running") => Shared.Data.ContainerStatus.Running,
        { } d when d.StartsWith("removing") => Shared.Data.ContainerStatus.Removing,
        { } d when d.StartsWith("paused") => Shared.Data.ContainerStatus.Paused,
        { } d when d.StartsWith("dead") => Shared.Data.ContainerStatus.Dead,
        _ => Shared.Data.ContainerStatus.NA
    };

    public static ContainerDetailedModel ToContainer(this ContainerInspectResponse response)
    {
        return new ContainerDetailedModel()
        {
            ContainerId = response.ID,
            ContainerName = response.Name,
            Status = MapStatus(response.State.Status),
            Created = response.Created,
            ImageName = response.Image,
            EntryPoint = response.Config.Entrypoint?.ToArray() ?? [],
            WorkingDirectory = response.Config.WorkingDir,
            Command = response.Config.Cmd.ToArray(),
            User = response.Config.User,
            Environment = ConvertEnvironment().Select(v => new KeyValue(v.Key, v.Value)).ToArray(),
            Labels = response.Config.Labels.Select(v => new KeyValue(v.Key, v.Value)).ToArray(),
            RestartCount = response.RestartCount,
            RestartPolicy = MapRestartPolicy(response.HostConfig.RestartPolicy.Name),
            Mounts = response.Mounts.Select(MapMountPoint).ToArray(),
            //Networks = response.NetworkSettings.
            //Ports = response.Config.Ports.Select(p => (p.PublicPort, p.PublicPort)).ToArray(), //todo
        };

        Dictionary<string, string> ConvertEnvironment()
        {
            return response
                .Config
                .Env
                .Where(d => d.Contains("="))
                .Select(d => d.Split("=") is {Length: > 1} array ? (array[0], string.Join("=", array.Skip(1))) : ("", ""))
                .DistinctBy(_ => _.Item1)
                .ToDictionary(_ => _.Item1, _ => _.Item2);
        }
    }

    public static Shared.Data.MountPoint MapMountPoint(this MountPoint point)
    {
        return new Shared.Data.MountPoint
        {
            Destination = point.Destination,
            Driver = point.Driver,
            Mode = point.Mode,
            Name = point.Name,
            Propagation = point.Propagation,
            RW = point.RW,
            Source = point.Source,
            Type = point.Type,
        };
    }

    public static Shared.Data.RestartPolicy MapRestartPolicy(this RestartPolicyKind name)
    {
        return name switch
        {
            RestartPolicyKind.Undefined => Shared.Data.RestartPolicy.Undefined,
            RestartPolicyKind.No => Shared.Data.RestartPolicy.No,
            RestartPolicyKind.Always => Shared.Data.RestartPolicy.Always,
            RestartPolicyKind.OnFailure => Shared.Data.RestartPolicy.OnFailure,
            RestartPolicyKind.UnlessStopped => Shared.Data.RestartPolicy.UnlessStopped,
            _ => Shared.Data.RestartPolicy.Undefined,
        };
    }

    public static ContainerModel ToContainer(this ContainerListResponse response)
    {
        return new ContainerModel()
        {
            ContainerId = response.ID,
            ContainerName = response.Names.First(), // todo,
            Status = MapStatus(response.State),
            Created = response.Created,
            ImageName = response.Image,
            Ports = response.Ports.Select(p => new ContainerPort(p.PublicPort, p.PrivatePort)).ToArray(), //todo
        };
    }

    public static IEnumerable<ImageModel> ToImages(this ImagesListResponse response)
    {
        foreach (var responseRepoTag in response.RepoTags)
        {
            var (name, tag) = ParseImage(responseRepoTag);   
            yield return new ImageModel()
            {
                ImageId = response.ID,
                Created = response.Created,
                ImageName = name,
                ImageTag = tag,
                Size = response.Size,
                Countainers = response.Containers
            };
        }
    }

    public static (string imageName, string imageTag) ParseImage(string image)
    {
        var tokens = image.Split(":");
        var (name, tag) = (tokens.First(), tokens.LastOrDefault());

        return (name, tag ?? string.Empty);
    }
}