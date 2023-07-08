using System;
using System.Collections.Generic;
using System.Text;

namespace nac.Forms.Reactive;

internal class SubjectDisposable<T> : IDisposable
{
    private List<IObserver<T>> m_store;
    private IObserver<T> m_self;

    public SubjectDisposable(List<IObserver<T>> store, IObserver<T> self)
    {
        m_store = store;
        m_self = self;
    }

    public void Dispose()
    {
        if (m_self == null) return;

        lock (m_store)
        {
            if (m_store.Contains(m_self))
            {
                m_store.Remove(m_self);
            }
            m_store = null;
            m_self = null;
        }
    }

}
