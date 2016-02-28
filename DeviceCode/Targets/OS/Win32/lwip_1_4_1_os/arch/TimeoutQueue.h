#pragma once
#include <cstdint>
#include <functional>
#include <utility>
#include <TinySupport.h>
#include <lwip/timers.h>

namespace Microsoft
{
    namespace Win32
    {
        ///<summary>Maintains an earliest deadline first ordered queue of timeout timers</summary>
        ///<remarks>
        /// This class keeps a queue of active timers sorted with the with the 
        /// earliest deadline first. When waiting on any synch object use
        /// WaitForMultipleObjectsEx() to wait for the synch obj AND the earliest
        /// deadline timeout so that the synch object, the timeout, or an I/O APC
        /// will exit the wait. If the timeout was signalled pop the action off the
        /// queue and call it's handler.
        ///</remarks>
        class TimeoutQueue
        { 
            // Using multimap instead of ordered_queue allows for removal of the timeout
            // from the middle to support cancellation.
            typedef std::multimap< uint64_t, Timeout > InternalMap_t;

        public:
            // iterator for the timeouts in the queue
            typedef InternalMap_t::iterator iterator;
            typedef InternalMap_t::const_iterator const_iterator;

            ///<summary>Push a new timeout onto the queue</summary>
            ///<param name="period">The period (in miliseconds of the timeout)</param>
            ///<param name="handler">LWIP Timeout handler function</param>
            ///<param name="arg">pointer to argument for the handler function</param>
            ///<param name="periodic">Flag to indicate if the timeout is periodic (see remarks section for details) [default=false]</param>
            ///<remarks>
            /// <para>This method will insert a timer into the queue with a timeout of <paramref cref="period">period</param> milliseconds.
            /// if periodic is false, the timeout is a simple one-shot timeout that expires after the timeout period. Otherwise the timeout
            /// is for a periodic timer that triggers and, automatically rexchedules itself to trigger again at the same interval until
            /// cancelled. When a periodic timeout is popped from the queue it's deadline value is automatically updated to reflect the 
            /// new deadline for the next trigger so that it can be inserted back into the queue.</para>
            ///<para>The handler and arg are used as an identifier for the timeout, in the event of cancellation they can be used
            /// with the find method to locate the specific timer and then remove it from the queue with the erase method.
            ///</remarks>
            void push( uint32_t period, sys_timeout_handler handler, void* arg, bool periodic = false )
            {
                Timeout to( period, handler, arg, periodic );
                auto element = MultiMap.emplace( to.get_Deadline(), std::move(to) );
            }

            iterator begin()
            {
                return MultiMap.begin();
            }
        
            iterator end()
            {
                return MultiMap.end();
            }

            /// <summary>removes the timout from the queue with the earliest deadline</summary>
            /// <remarks>
            /// <para>This removes the earliest timeout entry from the queue. Generaly this is a
            /// timeout that has expired, though that is not enforced here. In the event that
            /// the queue is empty this is a noop, so it is safe to call without needing to 
            /// check the queue as empty.</para> 
            /// <para>If the timeout at the head of the queue is a periodic one the timeout's
            /// deadline is updated to the next deadline and the timeout is re-inserted into
            /// the queue (typically it goies on the end but since the queue is sorted based
            /// on the earliest deadline it may end up in the middle, or even back at the head
            /// of the list if all other timeouts are for a later point in time.)
            /// </remarks>
            void pop()
            {
                if( MultiMap.empty() )
                    return;
                
                auto firstEntry = MultiMap.begin();

                // NOTE: use of std::move here uses move construction instead
                // of copy construction to prevent aliasing or duplicating the
                // underlying handle. using std::move and the move constructor
                // ensures the ownership of the handle is transferred without
                // actually aliasing, or destroying it.
                Timeout to( std::move( firstEntry->second ) );
                MultiMap.erase( firstEntry );
                if( to.IsPeriodic() )
                {
                    MultiMap.emplace( to.get_Deadline(), std::move( to ) );
                }
            }

            iterator find( sys_timeout_handler handler, void* arg )
            {
                for( auto it = MultiMap.begin(); it != MultiMap.end(); ++it )
                {
                    if( it->second.Match( handler, arg ) )
                        return it;
                }

                return end();
            }

            iterator erase( const_iterator it )
            {
                return MultiMap.erase( it );
            }

        private:
            InternalMap_t MultiMap;
        };
    }
}
