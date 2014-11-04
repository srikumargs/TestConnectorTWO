#pragma once
#include <cassert>



// The CSharedPtr class template stores a pointer to a dynamically
// allocated object, typically with a C++ new-expression.  The object
// pointed to is guaranteed to be deleted when the last CSharedPtr
// pointing to it is destroyed or reset.
//
// Every CSharedPtr meets the CopyConstructible and Assignable
// requirements of the C++ Standard Library, and so can be used
// in standard library containers.  Comparison operators are
// supplied so that CSharedPtr works with the standard library's
// associative containers.
//
// The CSharedPtr class is essentially a less robust and less portable
// version of the boost library shared_ptr (http://www.boost.org/libs/smart_ptr/shared_ptr.htm).
// This pared-down version can be desirable because it also has significantly
// fewer header dependencies (i.e., in order to utilize just the boost::shared_ptr,
// one must install ~17 boost header files into the development environment).




template<typename X>
class CSharedPtr
{
   public:
      CSharedPtr() :
         Proxy(0)
      {}

      CSharedPtr(X* p) :
         Proxy(p ? new _Proxy(p) : 0)
      {}

      CSharedPtr(const CSharedPtr<X>& b)
      {
         if((Proxy = b.Proxy) != 0)
         {
            Proxy->RefCount++;
         }
      }

      ~CSharedPtr()
      {
         Free();
      }

      const CSharedPtr<X>& operator=(const CSharedPtr<X>&);
      const CSharedPtr<X>& operator=(X*);
      bool operator==(const CSharedPtr<X>& rhs) const;
      bool operator!=(const CSharedPtr<X>& rhs) const;
      bool operator<(const CSharedPtr<X>& rhs) const;
      bool operator<=(const CSharedPtr<X>& rhs) const;
      bool operator>(const CSharedPtr<X>& rhs) const;
      bool operator>=(const CSharedPtr<X>& rhs) const;

      X* operator->() const
      {
         assert(Proxy);
         return get();
      }

      X& operator*() const
      {
         assert(Proxy);
         return *(Proxy->ValuePtr);
      }

      operator X*() const
      {
         return Proxy ? Proxy->ValuePtr : 0;
      }

      bool Shared() const
      {
         return (Proxy ? (Proxy->RefCount > 1) : false);
      }

      X* detach()
      {
         X* pX = NULL;
         if(NULL != Proxy)
         {
            pX = Proxy->ValuePtr;
            delete Proxy;
            Proxy = NULL;
         }
         return pX;
      }

      X* get() const
      {
         return Proxy ? Proxy->ValuePtr : 0;
      }


   private:
      class _Proxy
      {
         public:
            _Proxy(X* px) :
               RefCount(1),
               ValuePtr(px)
            {
               assert(ValuePtr);
            }

            unsigned short RefCount;
            X*             ValuePtr;
      };

      void Free();

      _Proxy* Proxy;
};




template <typename X>
inline const CSharedPtr<X>& CSharedPtr<X>::operator=(const CSharedPtr<X>& b)
{
   if(this != &b)
   {
      if(b.Proxy)
      {
         b.Proxy->RefCount++;
      }
      Free();
      Proxy = b.Proxy;
   }
   return *this;
}




template <typename X>
inline const CSharedPtr<X>& CSharedPtr<X>::operator=(X* p)
{
   if(Proxy && (p == Proxy->ValuePtr))
   {
      return *this;
   }

   Free();
   if(p)
   {
      Proxy = new _Proxy(p);
   }
   else
   {
      Proxy = 0;
   }

   return *this;
}




template <typename X>
inline bool CSharedPtr<X>::operator==(const CSharedPtr<X>& rhs) const
{
   // Need to make sure there are Proxy pointers before dereferencing them
   if(Proxy && rhs.Proxy)
   {
      return (Proxy->ValuePtr == rhs.Proxy->ValuePtr);
   }
   else
   {
      return Proxy == rhs.Proxy;
   }
}




template <typename X>
inline bool CSharedPtr<X>::operator!=(const CSharedPtr<X>& rhs) const
{
   return !(operator==(rhs));
}




template <typename X>
inline bool CSharedPtr<X>::operator<(const CSharedPtr<X>& rhs) const
{
   // Need to make sure there are Proxy pointers before dereferencing them
   if(Proxy && rhs.Proxy)
   {
      return (Proxy->ValuePtr < rhs.Proxy->ValuePtr);
   }
   else if(Proxy)  // ptr < 0
   {
      return 0;
   }
   else if(rhs.Proxy) // 0 < ptr
   {
      return (rhs.Proxy->ValuePtr ? 1 : 0);
   }
   else
   {
      return 0;
   }
}




template <typename X>
inline bool CSharedPtr<X>::operator<=(const CSharedPtr<X>& rhs) const
{
   return operator<(rhs) || operator==(rhs);
}




template <typename X>
inline bool CSharedPtr<X>::operator>(const CSharedPtr<X>& rhs) const
{
   return !(operator<=(rhs));
}




template <typename X>
inline bool CSharedPtr<X>::operator>=(const CSharedPtr<X>& rhs) const
{
   return !(operator<(rhs));
}




template <typename X>
void CSharedPtr<X>::Free()
{
   if(Proxy && --Proxy->RefCount == 0)
   {
      delete Proxy->ValuePtr;
      delete Proxy;
   }
}
