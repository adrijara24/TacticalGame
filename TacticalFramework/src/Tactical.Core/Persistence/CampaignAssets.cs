using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace Tactical.Core.Persistence
{

    public interface IPersistent
    {
        JsonObject ToJson();
        void FromJson(JsonObject json);
    }

    public interface IAsset : IPersistent
    {
        string ID { get; }
        string Name { get; }
        string Description { get; }
    }

    public class CampaignAssets
    {
        private readonly Dictionary<Type, Dictionary<string, IAsset>> assets = new();

        private static Type GetStorageType(Type type)
        {
            if (typeof(DItem).IsAssignableFrom(type))
                return typeof(DItem);

            return type;
        }

        public void Add<T>(T asset) where T : IAsset
        {
            ArgumentNullException.ThrowIfNull(asset);

            Type storageType = GetStorageType(typeof(T));

            if (!assets.TryGetValue(storageType, out var assetsOfType))
            {
                assetsOfType = new Dictionary<string, IAsset>();
                assets[storageType] = assetsOfType;
            }

            if (!assetsOfType.TryAdd(asset.ID, asset))
                throw new ArgumentException(
                    $"An asset in the {storageType.Name} group with ID '{asset.ID}' already exists.");
        }

        public void Replace<T>(string originalID, T asset) where T : IAsset
        {
            ArgumentNullException.ThrowIfNull(asset);

            Type storageType = GetStorageType(typeof(T));

            if (!assets.TryGetValue(storageType, out var assetsOfType))
                throw new KeyNotFoundException($"No assets of type {storageType.Name} have been registered.");

            if (!assetsOfType.TryGetValue(originalID, out var originalAsset))
                throw new KeyNotFoundException($"No asset with ID '{originalID}' exists.");

            if (originalAsset is not T)
                throw new InvalidCastException($"Asset '{originalID}' is a {originalAsset.GetType().Name}, not a {typeof(T).Name}.");

            if (originalID != asset.ID && assetsOfType.ContainsKey(asset.ID))
                throw new ArgumentException($"An asset in the {storageType.Name} group with ID '{asset.ID}' already exists.");

            assetsOfType.Remove(originalID);
            assetsOfType.Add(asset.ID, asset);
        }

        public T ByTag<T>(string id) where T : IAsset
        {
            Type storageType = GetStorageType(typeof(T));

            if (!assets.TryGetValue(storageType, out var assetsOfType))
                throw new KeyNotFoundException(
                    $"No assets of type {storageType.Name} have been registered.");

            if (!assetsOfType.TryGetValue(id, out var asset))
                throw new KeyNotFoundException(
                    $"No asset with ID '{id}' exists.");

            if (asset is not T result)
                throw new InvalidCastException(
                    $"Asset '{id}' is a {asset.GetType().Name}, not a {typeof(T).Name}.");

            return result;
        }

        public bool Contains<T>(string id) where T : IAsset
        {
            Type storageType = GetStorageType(typeof(T));

            return assets.TryGetValue(storageType, out var assetsOfType)
                && assetsOfType.TryGetValue(id, out var asset)
                && asset is T;
        }

        public bool Remove<T>(string id) where T : IAsset
        {
            Type storageType = GetStorageType(typeof(T));

            if (!assets.TryGetValue(storageType, out var assetsOfType))
                return false;

            if (!assetsOfType.TryGetValue(id, out var asset) || asset is not T)
                return false;

            return assetsOfType.Remove(id);
        }

        public IReadOnlyCollection<T> GetAll<T>() where T : IAsset
        {
            Type storageType = GetStorageType(typeof(T));

            if (!assets.TryGetValue(storageType, out var assetsOfType))
                return Array.Empty<T>();

            return assetsOfType.Values.OfType<T>().ToArray();
        }

        public IEnumerable<IAsset> GetAllAssets()
        {
            return assets.Values.SelectMany(x => x.Values);
        }
    }
}