using System;
using System.Collections.Generic;
using System.Text;

namespace nac.Forms.Reactive;

internal class SubjectDisposable<T> : IDisposable
{
    private Dictionary<IObserver<T>, int> m_store;
    private IObserver<T> m_self;

    public SubjectDisposable(Dictionary<IObserver<T>, int> store, IObserver<T> self)
    {
        m_store = store;
        m_self = self;
    }

    public void Dispose()
    {
        if (m_self == null) return;

        lock (m_store)
        {
            if (m_store.TryGetValue(m_self, out var cnt))
            {
                cnt--;
                if (cnt > 0) m_store[m_self] = cnt;
                else m_store.Remove(m_self);
            }
            m_store = null;
            m_self = null;
        }
    }

}
