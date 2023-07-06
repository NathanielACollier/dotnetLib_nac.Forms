using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nac.Forms.Reactive;

internal class Subject<T> : IObservable<T>, IObserver<T>
{
    private Dictionary<IObserver<T>, int> m_observers;

    public Subject()
    {
        m_observers = new Dictionary<IObserver<T>, int>();
    }

    public void OnCompleted()
    {
        IObserver<T>[] arr = null;
        lock (m_observers)
        {
            arr = m_observers.Keys.ToArray(m_observers.Count);
        }
        foreach (var obs in arr)
        {
            obs.OnCompleted();
        }
    }

    public void OnNext(T value)
    {
        IObserver<T>[] arr = null;
        lock (m_observers)
        {
            arr = m_observers.Keys.ToArray(m_observers.Count);
        }
        foreach (var obs in arr)
        {
            obs.OnNext(value);
        }
    }

    public void OnError(Exception e)
    {
        IObserver<T>[] arr = null;
        lock (m_observers)
        {
            arr = m_observers.Keys.ToArray(m_observers.Count);
        }
        foreach (var obs in arr)
        {
            obs.OnError(e);
        }
    }

    public IDisposable Subscribe(IObserver<T> obs)
    {
        lock (m_observers)
        {
            if (m_observers.TryGetValue(obs, out var cnt))
            {
                m_observers[obs] = cnt + 1;
            }
            else
            {
                m_observers[obs] = 1;
            }

            return new SubjectDisposable<T>(m_observers, obs);
        }
    }

}



