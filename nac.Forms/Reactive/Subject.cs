using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nac.Forms.Reactive;

internal class Subject<T> : IObservable<T>, IObserver<T>
{
    private List<IObserver<T>> observers;

    public Subject()
    {
        this.observers = new List<IObserver<T>>();
    }

    public void OnCompleted()
    {
        IObserver<T>[] arr = null;
        lock (this.observers)
        {
            arr = this.observers.ToArray();
        }
        foreach (var obs in arr)
        {
            obs.OnCompleted();
        }
    }

    public void OnNext(T value)
    {
        IObserver<T>[] arr = null;
        lock (this.observers)
        {
            arr = this.observers.ToArray();
        }
        foreach (var obs in arr)
        {
            obs.OnNext(value);
        }
    }

    public void OnError(Exception e)
    {
        IObserver<T>[] arr = null;
        lock (this.observers)
        {
            arr = this.observers.ToArray();
        }
        foreach (var obs in arr)
        {
            obs.OnError(e);
        }
    }

    public IDisposable Subscribe(IObserver<T> obs)
    {
        lock (this.observers)
        {
            if (!this.observers.Contains(obs))
            {
                this.observers.Add(obs);
            }

            return new SubjectDisposable<T>(this.observers, obs);
        }
    }

}



