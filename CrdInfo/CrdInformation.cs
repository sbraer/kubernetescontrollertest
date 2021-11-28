using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace CrdInfo;
public class CrdInformation : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<CrdItem> _items = new();
    private bool disposedValue;

    public void AddCrdConfiguration(string deployment, string configMapName, string uid)
    {
        _lock.EnterWriteLock();
        try
        {
            var item = _items.Where(t => t.Uid == uid).SingleOrDefault();
            if (item != null)
            {
                item.Deployment = deployment;
                item.ConfigMapName = configMapName;
            }
            else
            {
                _items.Add(new CrdItem(deployment, configMapName, uid));
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public void DeleteCrdConfiguration(string uid)
    {
        _lock.EnterWriteLock();
        try
        {
            var toDelete = _items.Where(t => t.Uid == uid).ToList();
            toDelete.ForEach(t => _items.Remove(t));
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public ImmutableArray<CrdItem> GetCrdFromUid(string uid)
    {
        _lock.EnterReadLock();
        try
        {
            return _items.Where(t => t.Uid == uid).ToImmutableArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public ImmutableArray<CrdItem> GetCrdFromConfigmapName(string configMapName)
    {
        _lock.EnterReadLock();
        try
        {
            return _items.Where(t => t.ConfigMapName == configMapName).ToImmutableArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public ImmutableArray<CrdItem> GetCrdFromDeployment(string deployment)
    {
        _lock.EnterReadLock();
        try
        {
            return _items.Where(t => t.Deployment == deployment).ToImmutableArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public ImmutableArray<CrdItem> GetCrdAll()
    {
        _lock.EnterReadLock();
        try
        {
            return _items.ToImmutableArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _lock.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
