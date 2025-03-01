using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
namespace Swipl
{
    
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    sealed class NativeTypeNameAttribute : Attribute
    {
        public NativeTypeNameAttribute(string name) { }
    }
    
    public enum TermType {
        PL_VARIABLE	= (1),		/* nothing */
        PL_ATOM		= (2),		/* const char * */
        PL_INTEGER	= (3),		/* int */
        PL_RATIONAL	= (4),		/* rational number */
        PL_FLOAT	= (5),		/* double */
        PL_STRING	= (6),		/* const char * */
        PL_TERM		= (7),
    }

    public partial struct __PL_module
    {
    }

    public partial struct __PL_procedure
    {
    }

    public partial struct __PL_record
    {
    }

    public partial struct __PL_queryRef
    {
    }

    public partial struct __PL_foreign_context
    {
    }

    public partial struct __PL_PL_local_data
    {
    }

    public partial struct io_stream
    {
    }


    [StructLayout(LayoutKind.Explicit)]
    public unsafe partial struct term_value_t
    {
        [FieldOffset(0)]
        [NativeTypeName("int64_t")]
        public long i;

        [FieldOffset(0)]
        public double f;

        [FieldOffset(0)]
        [NativeTypeName("char *")]
        public sbyte* s;

        [FieldOffset(0)]
        [NativeTypeName("atom_t")]
        public nuint a;

        [FieldOffset(0)]
        [NativeTypeName("__AnonymousRecord_SWI-Prolog_L229_C3")]
        public _t_e__Struct t;

        public partial struct _t_e__Struct
        {
            [NativeTypeName("atom_t")]
            public nuint name;

            [NativeTypeName("size_t")]
            public nuint arity;
        }
    }

    public unsafe partial struct PL_extension
    {
        [NativeTypeName("const char *")]
        public IntPtr predicate_name;

        public short arity;

        [NativeTypeName("pl_function_t")]
        public void* function;

        public short flags;
    }

    public unsafe partial struct PL_blob_t
    {
        [NativeTypeName("uintptr_t")]
        public nuint magic;

        [NativeTypeName("uintptr_t")]
        public nuint flags;

        [NativeTypeName("const char *")]
        public IntPtr name;

        [NativeTypeName("int (*)(atom_t)")]
        public delegate* unmanaged[Cdecl]<nuint, int> release;

        [NativeTypeName("int (*)(atom_t, atom_t)")]
        public delegate* unmanaged[Cdecl]<nuint, nuint, int> compare;

        [NativeTypeName("int (*)(IOSTREAM *, atom_t, int)")]
        public delegate* unmanaged[Cdecl]<io_stream*, nuint, int, int> write;

        [NativeTypeName("void (*)(atom_t)")]
        public delegate* unmanaged[Cdecl]<nuint, void> acquire;

        [NativeTypeName("int (*)(atom_t, IOSTREAM *)")]
        public delegate* unmanaged[Cdecl]<nuint, io_stream*, int> save;

        [NativeTypeName("atom_t (*)(IOSTREAM *)")]
        public delegate* unmanaged[Cdecl]<io_stream*, nuint> load;

        [NativeTypeName("size_t")]
        public nuint padding;

        [NativeTypeName("void *[9]")]
        public _reserved_e__FixedBuffer reserved;

        public int registered;

        public int rank;

        [NativeTypeName("struct PL_blob_t *")]
        public PL_blob_t* next;

        [NativeTypeName("atom_t")]
        public nuint atom_name;

        public unsafe partial struct _reserved_e__FixedBuffer
        {
            public void* e0;
            public void* e1;
            public void* e2;
            public void* e3;
            public void* e4;
            public void* e5;
            public void* e6;
            public void* e7;
            public void* e8;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    public enum _PL_opt_enum_t
    {
        _OPT_END = -1,
        OPT_BOOL = 0,
        OPT_INT,
        OPT_INT64,
        OPT_UINT64,
        OPT_SIZE,
        OPT_DOUBLE,
        OPT_STRING,
        OPT_ATOM,
        OPT_TERM,
        OPT_LOCALE,
    }

    public partial struct PL_option_t
    {
        [NativeTypeName("atom_t")]
        public nuint name;

        public _PL_opt_enum_t type;

        [NativeTypeName("const char *")]
        public IntPtr @string;
    }

    public unsafe partial struct pl_sigaction
    {
        [NativeTypeName("void (*)(int)")]
        public delegate* unmanaged[Cdecl]<int, void> sa_cfunction;

        [NativeTypeName("predicate_t")]
        public __PL_procedure* sa_predicate;

        public int sa_flags;

        [NativeTypeName("void *[2]")]
        public _reserved_e__FixedBuffer reserved;

        public unsafe partial struct _reserved_e__FixedBuffer
        {
            public void* e0;
            public void* e1;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    [NativeTypeName("unsigned int")]
    public enum rc_cancel : uint
    {
        PL_THREAD_CANCEL_FAILED = (0),
        PL_THREAD_CANCEL_JOINED = (1),
        PL_THREAD_CANCEL_MUST_JOIN,
    }

    public unsafe partial struct PL_thread_attr_t
    {
        [NativeTypeName("size_t")]
        public nuint stack_limit;

        [NativeTypeName("size_t")]
        public nuint table_space;

        [NativeTypeName("char *")]
        public sbyte* alias;

        [NativeTypeName("rc_cancel (*)(int)")]
        public delegate* unmanaged[Cdecl]<int, rc_cancel> cancel;

        [NativeTypeName("intptr_t")]
        public nint flags;

        [NativeTypeName("size_t")]
        public nuint max_queue_size;

        [NativeTypeName("void *[3]")]
        public _reserved_e__FixedBuffer reserved;

        public unsafe partial struct _reserved_e__FixedBuffer
        {
            public void* e0;
            public void* e1;
            public void* e2;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    public partial struct __PL_table
    {
    }

    public partial struct __PL_table_enum
    {
    }

    public unsafe partial struct PL_prof_type_t
    {
        [NativeTypeName("int (*)(term_t, void *)")]
        public delegate* unmanaged[Cdecl]<nuint, void*, int> unify;

        [NativeTypeName("int (*)(term_t, void **)")]
        public delegate* unmanaged[Cdecl]<nuint, void**, int> get;

        [NativeTypeName("void (*)(int)")]
        public delegate* unmanaged[Cdecl]<int, void> activate;

        [NativeTypeName("void (*)(void *)")]
        public delegate* unmanaged[Cdecl]<void*, void> release;

        [NativeTypeName("void *[4]")]
        public _dummy_e__FixedBuffer dummy;

        [NativeTypeName("intptr_t")]
        public nint magic;

        public unsafe partial struct _dummy_e__FixedBuffer
        {
            public void* e0;
            public void* e1;
            public void* e2;
            public void* e3;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    public partial struct xpceref_t
    {
        public int type;

        [NativeTypeName("__AnonymousRecord_SWI-Prolog_L1421_C3")]
        public _value_e__Union value;

        [StructLayout(LayoutKind.Explicit)]
        public partial struct _value_e__Union
        {
            [FieldOffset(0)]
            [NativeTypeName("uintptr_t")]
            public nuint i;

            [FieldOffset(0)]
            [NativeTypeName("atom_t")]
            public nuint a;
        }
    }
    
    

    public unsafe partial struct pl_context_t
    {
        [NativeTypeName("PL_engine_t")]
        public __PL_PL_local_data* ld;

        [NativeTypeName("struct __PL_queryFrame *")]
        public __PL_queryFrame* qf;

        [NativeTypeName("struct __PL_localFrame *")]
        public __PL_localFrame* fr;

        [NativeTypeName("__PL_code *")]
        public nuint* pc;

        [NativeTypeName("void *[10]")]
        public _reserved_e__FixedBuffer reserved;

        public partial struct __PL_queryFrame
        {
        }

        public partial struct __PL_localFrame
        {
        }

        public unsafe partial struct _reserved_e__FixedBuffer
        {
            public void* e0;
            public void* e1;
            public void* e2;
            public void* e3;
            public void* e4;
            public void* e5;
            public void* e6;
            public void* e7;
            public void* e8;
            public void* e9;

            public ref void* this[int index]
            {
                get
                {
                    fixed (void** pThis = &e0)
                    {
                        return ref pThis[index];
                    }
                }
            }
        }
    }

    public static unsafe partial class Methods
    {
        private const string Lib = "libswipl";
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("foreign_t")]
        public static extern nuint _PL_retry([NativeTypeName("intptr_t")] nint param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("foreign_t")]
        public static extern nuint _PL_retry_address(void* param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("foreign_t")]
        public static extern nuint _PL_yield_address(void* param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_foreign_control([NativeTypeName("control_t")] __PL_foreign_context* param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("intptr_t")]
        public static extern nint PL_foreign_context([NativeTypeName("control_t")] __PL_foreign_context* param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_foreign_context_address([NativeTypeName("control_t")] __PL_foreign_context* param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("predicate_t")]
        public static extern __PL_procedure* PL_foreign_context_predicate([NativeTypeName("control_t")] __PL_foreign_context* param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_register_extensions([NativeTypeName("const PL_extension *")] PL_extension* e);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_register_extensions_in_module([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string module, [NativeTypeName("const PL_extension *")] PL_extension* e);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_register_foreign([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string name, int arity, [NativeTypeName("pl_function_t")] void* func, int flags, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_register_foreign_in_module([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string module, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string name, int arity, [NativeTypeName("pl_function_t")] void* func, int flags, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_load_extensions([NativeTypeName("const PL_extension *")] PL_extension* e);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_license([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string license, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string module);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("module_t")]
        public static extern __PL_module* PL_context();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint PL_module_name([NativeTypeName("module_t")] __PL_module* module);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("module_t")]
        public static extern __PL_module* PL_new_module([NativeTypeName("atom_t")] nuint name);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_strip_module([NativeTypeName("term_t")] nuint @in, [NativeTypeName("module_t *")] __PL_module** m, [NativeTypeName("term_t")] nuint @out);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("const atom_t *")]
        public static extern nuint* _PL_atoms();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("PL_fid_t")]
        public static extern nuint PL_open_foreign_frame();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_rewind_foreign_frame([NativeTypeName("PL_fid_t")] nuint cid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_close_foreign_frame([NativeTypeName("PL_fid_t")] nuint cid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_discard_foreign_frame([NativeTypeName("PL_fid_t")] nuint cid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("predicate_t")]
        public static extern __PL_procedure* PL_pred([NativeTypeName("functor_t")] nuint f, [NativeTypeName("module_t")] __PL_module* m);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("predicate_t")]
        public static extern __PL_procedure* PL_predicate([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string name, int arity, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string module);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_predicate_info([NativeTypeName("predicate_t")] __PL_procedure* pred, [NativeTypeName("atom_t *")] nuint* name, [NativeTypeName("size_t *")] nuint* arity, [NativeTypeName("module_t *")] __PL_module** module);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("qid_t")]
        public static extern __PL_queryRef* PL_open_query([NativeTypeName("module_t")] __PL_module* m, int flags, [NativeTypeName("predicate_t")] __PL_procedure* pred, [NativeTypeName("term_t")] nuint t0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_next_solution([NativeTypeName("qid_t")] __PL_queryRef* qid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_close_query([NativeTypeName("qid_t")] __PL_queryRef* qid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cut_query([NativeTypeName("qid_t")] __PL_queryRef* qid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("qid_t")]
        public static extern __PL_queryRef* PL_current_query();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("PL_engine_t")]
        public static extern __PL_PL_local_data* PL_query_engine([NativeTypeName("qid_t")] __PL_queryRef* qid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_can_yield();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_call([NativeTypeName("term_t")] nuint t, [NativeTypeName("module_t")] __PL_module* m);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_call_predicate([NativeTypeName("module_t")] __PL_module* m, int flags, [NativeTypeName("predicate_t")] __PL_procedure* pred, [NativeTypeName("term_t")] nuint t0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("term_t")]
        public static extern nuint PL_exception([NativeTypeName("qid_t")] __PL_queryRef* qid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_raise_exception([NativeTypeName("term_t")] nuint exception);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_throw([NativeTypeName("term_t")] nuint exception);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_clear_exception();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("term_t")]
        public static extern nuint PL_yielded([NativeTypeName("qid_t")] __PL_queryRef* qid);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_assert([NativeTypeName("term_t")] nuint term, [NativeTypeName("module_t")] __PL_module* m, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("term_t")]
        public static extern nuint PL_new_term_refs([NativeTypeName("size_t")] nuint n);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("term_t")]
        public static extern nuint PL_new_term_ref();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("term_t")]
        public static extern nuint PL_copy_term_ref([NativeTypeName("term_t")] nuint from);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_free_term_ref([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_reset_term_refs([NativeTypeName("term_t")] nuint r);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint PL_new_atom([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint PL_new_atom_nchars([NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint PL_new_atom_wchars([NativeTypeName("size_t")] nuint len, [NativeTypeName("const pl_wchar_t *")] uint* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint PL_new_atom_mbchars(int rep, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern IntPtr PL_atom_chars([NativeTypeName("atom_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern IntPtr PL_atom_nchars([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_atom_mbchars([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        [return: NativeTypeName("const wchar_t *")]
        public static extern uint* PL_atom_wchars([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_register_atom([NativeTypeName("atom_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_unregister_atom([NativeTypeName("atom_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("functor_t")]
        public static extern nuint PL_new_functor_sz([NativeTypeName("atom_t")] nuint f, [NativeTypeName("size_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("functor_t")]
        public static extern nuint PL_new_functor([NativeTypeName("atom_t")] nuint f, int a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint PL_functor_name([NativeTypeName("functor_t")] nuint f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_functor_arity([NativeTypeName("functor_t")] nuint f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("size_t")]
        public static extern nuint PL_functor_arity_sz([NativeTypeName("functor_t")] nuint f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_atom([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_bool([NativeTypeName("term_t")] nuint t, int* value);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_atom_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("char **")] sbyte** a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_string([NativeTypeName("term_t")] nuint t, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("size_t *")] nuint* len);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_list_chars([NativeTypeName("term_t")] nuint l, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_atom_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_list_nchars([NativeTypeName("term_t")] nuint l, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("char **")] sbyte** s, [NativeTypeName("unsigned int")] uint flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_integer([NativeTypeName("term_t")] nuint t, int* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_long([NativeTypeName("term_t")] nuint t, [NativeTypeName("long *")] nint* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_intptr([NativeTypeName("term_t")] nuint t, [NativeTypeName("intptr_t *")] nint* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_pointer([NativeTypeName("term_t")] nuint t, void** ptr);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_float([NativeTypeName("term_t")] nuint t, double* f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t *")] nuint* f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_name_arity_sz([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, [NativeTypeName("size_t *")] nuint* arity);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_compound_name_arity_sz([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, [NativeTypeName("size_t *")] nuint* arity);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_name_arity([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, int* arity);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_compound_name_arity([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* name, int* arity);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_module([NativeTypeName("term_t")] nuint t, [NativeTypeName("module_t *")] __PL_module** module);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_arg_sz([NativeTypeName("size_t")] nuint index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_arg(int index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_dict_key([NativeTypeName("atom_t")] nuint key, [NativeTypeName("term_t")] nuint dict, [NativeTypeName("term_t")] nuint value);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_list([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_head([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_tail([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_nil([NativeTypeName("term_t")] nuint l);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_term_value([NativeTypeName("term_t")] nuint t, term_value_t* v);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("char *")]
        public static extern sbyte* PL_quote(int chr, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string data);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_for_dict([NativeTypeName("term_t")] nuint dict, [NativeTypeName("int (*)(term_t, term_t, void *)")] delegate* unmanaged[Cdecl]<nuint, nuint, void*, int> func, void* closure, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_term_type([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_variable([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_ground([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_atom([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_integer([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_string([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_float([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_rational([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_compound([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_callable([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_list([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_dict([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_pair([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_atomic([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_number([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_acyclic([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_variable([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_atom([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_bool([NativeTypeName("term_t")] nuint t, int val);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_atom_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_string_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_chars([NativeTypeName("term_t")] nuint t, int flags, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_list_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_list_codes([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_atom_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_string_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_list_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_list_ncodes([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_integer([NativeTypeName("term_t")] nuint t, [NativeTypeName("long")] nint i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_pointer([NativeTypeName("term_t")] nuint t, void* ptr);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_float([NativeTypeName("term_t")] nuint t, double f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint functor);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_list([NativeTypeName("term_t")] nuint l);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_nil([NativeTypeName("term_t")] nuint l);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_term([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_dict([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint tag, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const atom_t *")] nuint* keys, [NativeTypeName("term_t")] nuint values);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint _PL_cons_small_int([NativeTypeName("int64_t")] long v);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void _PL_unregister_keys([NativeTypeName("size_t")] nuint len, [NativeTypeName("atom_t *")] nuint* keys);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cons_functor([NativeTypeName("term_t")] nuint h, [NativeTypeName("functor_t")] nuint f, __arglist);
        
        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cons_functor_v([NativeTypeName("term_t")] nuint h, [NativeTypeName("functor_t")] nuint fd, [NativeTypeName("term_t")] nuint a0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cons_list([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_atom([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_atom_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_list_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_list_codes([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_string_chars([NativeTypeName("term_t")] nuint t, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_atom_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_list_ncodes([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_list_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint l, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_string_nchars([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_bool([NativeTypeName("term_t")] nuint t, int n);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_integer([NativeTypeName("term_t")] nuint t, [NativeTypeName("intptr_t")] nint n);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_float([NativeTypeName("term_t")] nuint t, double f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_pointer([NativeTypeName("term_t")] nuint t, void* ptr);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_functor([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_compound([NativeTypeName("term_t")] nuint t, [NativeTypeName("functor_t")] nuint f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_list([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_nil([NativeTypeName("term_t")] nuint l);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_arg_sz([NativeTypeName("size_t")] nuint index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_arg(int index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_term([NativeTypeName("term_t")] nuint t, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_chars([NativeTypeName("term_t")] nuint t, int flags, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_skip_list([NativeTypeName("term_t")] nuint list, [NativeTypeName("term_t")] nuint tail, [NativeTypeName("size_t *")] nuint* len);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        public static extern int PL_put_wchars([NativeTypeName("term_t")] nuint t, int type, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const pl_wchar_t *")] uint* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        public static extern int PL_unify_wchars([NativeTypeName("term_t")] nuint t, int type, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const pl_wchar_t *")] uint* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        public static extern int PL_unify_wchars_diff([NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint tail, int type, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const pl_wchar_t *")] uint* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        public static extern int PL_get_wchars([NativeTypeName("term_t")] nuint l, [NativeTypeName("size_t *")] nuint* length, [NativeTypeName("pl_wchar_t **")] uint** s, [NativeTypeName("unsigned int")] uint flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("size_t")]
        public static extern nuint PL_utf8_strlen([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s, [NativeTypeName("size_t")] nuint len);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_int64([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t *")] long* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_uint64([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t *")] ulong* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_int64([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t")] long value);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_uint64([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t")] ulong value);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_int64([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t")] long i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_uint64([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t")] ulong i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_attvar([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_attr([NativeTypeName("term_t")] nuint v, [NativeTypeName("term_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_delay_list([NativeTypeName("term_t")] nuint l);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_atom_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t *")] nuint* a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_integer_ex([NativeTypeName("term_t")] nuint t, int* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_long_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("long *")] nint* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_int64_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("int64_t *")] long* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_uint64_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("uint64_t *")] ulong* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_intptr_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("intptr_t *")] nint* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_size_ex([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_bool_ex([NativeTypeName("term_t")] nuint t, int* i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_float_ex([NativeTypeName("term_t")] nuint t, double* f);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_char_ex([NativeTypeName("term_t")] nuint t, int* p, int eof);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_bool_ex([NativeTypeName("term_t")] nuint t, int val);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_pointer_ex([NativeTypeName("term_t")] nuint t, void** addrp);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_list_ex([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_nil_ex([NativeTypeName("term_t")] nuint l);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_list_ex([NativeTypeName("term_t")] nuint l, [NativeTypeName("term_t")] nuint h, [NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_nil_ex([NativeTypeName("term_t")] nuint l);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_instantiation_error([NativeTypeName("term_t")] nuint culprit);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_uninstantiation_error([NativeTypeName("term_t")] nuint culprit);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_representation_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string resource);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_type_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string expected, [NativeTypeName("term_t")] nuint culprit);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_domain_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string expected, [NativeTypeName("term_t")] nuint culprit);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_existence_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string type, [NativeTypeName("term_t")] nuint culprit);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_permission_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string operation, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string type, [NativeTypeName("term_t")] nuint culprit);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_resource_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string resource);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_syntax_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string msg, [NativeTypeName("IOSTREAM *")] io_stream* @in);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_blob([NativeTypeName("term_t")] nuint t, PL_blob_t** type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_blob([NativeTypeName("term_t")] nuint t, void* blob, [NativeTypeName("size_t")] nuint len, PL_blob_t* type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("atom_t")]
        public static extern nuint PL_new_blob(void* blob, [NativeTypeName("size_t")] nuint len, PL_blob_t* type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_blob([NativeTypeName("term_t")] nuint t, void* blob, [NativeTypeName("size_t")] nuint len, PL_blob_t* type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_blob([NativeTypeName("term_t")] nuint t, void** blob, [NativeTypeName("size_t *")] nuint* len, PL_blob_t** type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_blob_data([NativeTypeName("atom_t")] nuint a, [NativeTypeName("size_t *")] nuint* len, [NativeTypeName("struct PL_blob_t **")] PL_blob_t** type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_free_blob([NativeTypeName("atom_t")] nuint blob);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_register_blob_type(PL_blob_t* type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern PL_blob_t* PL_find_blob_type([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unregister_blob_type(PL_blob_t* type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_file_name([NativeTypeName("term_t")] nuint n, [NativeTypeName("char **")] sbyte** name, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        public static extern int PL_get_file_nameW([NativeTypeName("term_t")] nuint n, [NativeTypeName("wchar_t **")] uint** name, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_changed_cwd();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("char *")]
        public static extern sbyte* PL_cwd([NativeTypeName("char *")] sbyte* buf, [NativeTypeName("size_t")] nuint buflen);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_bool([NativeTypeName("term_t")] nuint p, int* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_char([NativeTypeName("term_t")] nuint p, [NativeTypeName("char *")] sbyte* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_schar([NativeTypeName("term_t")] nuint p, [NativeTypeName("signed char *")] sbyte* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_uchar([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned char *")] byte* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_short([NativeTypeName("term_t")] nuint p, short* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_ushort([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned short *")] ushort* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_int([NativeTypeName("term_t")] nuint p, int* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_uint([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned int *")] uint* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_long([NativeTypeName("term_t")] nuint p, [NativeTypeName("long *")] nint* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_ulong([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned long *")] nuint* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_llong([NativeTypeName("term_t")] nuint p, [NativeTypeName("long long *")] long* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_ullong([NativeTypeName("term_t")] nuint p, [NativeTypeName("unsigned long long *")] ulong* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_int32([NativeTypeName("term_t")] nuint p, [NativeTypeName("int32_t *")] int* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_uint32([NativeTypeName("term_t")] nuint p, [NativeTypeName("uint32_t *")] uint* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_int64([NativeTypeName("term_t")] nuint p, [NativeTypeName("int64_t *")] long* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_uint64([NativeTypeName("term_t")] nuint p, [NativeTypeName("uint64_t *")] ulong* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_size_t([NativeTypeName("term_t")] nuint p, [NativeTypeName("size_t *")] nuint* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_float([NativeTypeName("term_t")] nuint p, double* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_single([NativeTypeName("term_t")] nuint p, float* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_string([NativeTypeName("term_t")] nuint p, [NativeTypeName("char **")] sbyte** c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_codes([NativeTypeName("term_t")] nuint p, [NativeTypeName("char **")] sbyte** c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_atom([NativeTypeName("term_t")] nuint p, [NativeTypeName("atom_t *")] nuint* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_i_address([NativeTypeName("term_t")] nuint p, void* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_o_int64([NativeTypeName("int64_t")] long c, [NativeTypeName("term_t")] nuint p);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_o_float(double c, [NativeTypeName("term_t")] nuint p);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_o_single(float c, [NativeTypeName("term_t")] nuint p);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_o_string([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string c, [NativeTypeName("term_t")] nuint p);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_o_codes([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string c, [NativeTypeName("term_t")] nuint p);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_o_atom([NativeTypeName("atom_t")] nuint c, [NativeTypeName("term_t")] nuint p);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_o_address(void* address, [NativeTypeName("term_t")] nuint p);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("term_t")]
        public static extern nuint PL_new_nil_ref();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_encoding();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cvt_set_encoding(int enc);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void SP_set_state(int state);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int SP_get_state();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_compare([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_same_compound([NativeTypeName("term_t")] nuint t1, [NativeTypeName("term_t")] nuint t2);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_warning([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string fmt, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_warningX([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string fmt, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_fatal_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string fmt, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_api_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string fmt, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_system_error([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string fmt, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_print_message([NativeTypeName("atom_t")] nuint severity, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("record_t")]
        public static extern __PL_record* PL_record([NativeTypeName("term_t")] nuint term);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_recorded([NativeTypeName("record_t")] __PL_record* record, [NativeTypeName("term_t")] nuint term);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_erase([NativeTypeName("record_t")] __PL_record* record);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("record_t")]
        public static extern __PL_record* PL_duplicate_record([NativeTypeName("record_t")] __PL_record* r);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("char *")]
        public static extern sbyte* PL_record_external([NativeTypeName("term_t")] nuint t, [NativeTypeName("size_t *")] nuint* size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_recorded_external([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string rec, [NativeTypeName("term_t")] nuint term);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_erase_external([NativeTypeName("char *")] sbyte* rec);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_set_prolog_flag([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string name, int type, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("PL_atomic_t")]
        public static extern nuint _PL_get_atomic([NativeTypeName("term_t")] nuint t);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void _PL_put_atomic([NativeTypeName("term_t")] nuint t, [NativeTypeName("PL_atomic_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int _PL_unify_atomic([NativeTypeName("term_t")] nuint t, [NativeTypeName("PL_atomic_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int _PL_get_arg_sz([NativeTypeName("size_t")] nuint index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int _PL_get_arg(int index, [NativeTypeName("term_t")] nuint t, [NativeTypeName("term_t")] nuint a);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_mark_string_buffers([NativeTypeName("buf_mark_t *")] nuint* mark);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_release_string_buffers_from_mark([NativeTypeName("buf_mark_t")] nuint mark);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_stream([NativeTypeName("term_t")] nuint t, [NativeTypeName("IOSTREAM *")] io_stream* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_stream_handle([NativeTypeName("term_t")] nuint t, [NativeTypeName("IOSTREAM **")] io_stream** s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_stream([NativeTypeName("term_t")] nuint t, [NativeTypeName("IOSTREAM **")] io_stream** s, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_stream_from_blob([NativeTypeName("atom_t")] nuint a, [NativeTypeName("IOSTREAM **")] io_stream** s, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("IOSTREAM *")]
        public static extern io_stream* PL_acquire_stream([NativeTypeName("IOSTREAM *")] io_stream* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_release_stream([NativeTypeName("IOSTREAM *")] io_stream* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_release_stream_noerror([NativeTypeName("IOSTREAM *")] io_stream* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("IOSTREAM *")]
        public static extern io_stream* PL_open_resource([NativeTypeName("module_t")] __PL_module* m, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string name, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string rc_class, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string mode);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("IOSTREAM **")]
        public static extern io_stream** _PL_streams();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_write_term([NativeTypeName("IOSTREAM *")] io_stream* s, [NativeTypeName("term_t")] nuint term, int precedence, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_ttymode([NativeTypeName("IOSTREAM *")] io_stream* s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_put_term_from_chars([NativeTypeName("term_t")] nuint t, int flags, [NativeTypeName("size_t")] nuint len, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string s);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_chars_to_term([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string chars, [NativeTypeName("term_t")] nuint term);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl,  ExactSpelling = true)]
        public static extern int PL_wchars_to_term([NativeTypeName("const pl_wchar_t *")] uint* chars, [NativeTypeName("term_t")] nuint term);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true,  BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int PL_initialise(int argc, [NativeTypeName("char **")] String[] argv);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int PL_winitialise(int argc, [NativeTypeName("wchar_t **")] uint** argv);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_is_initialised(int* argc, [NativeTypeName("char ***")] sbyte*** argv);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_set_resource_db_mem([NativeTypeName("const unsigned char *")] byte* data, [NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_toplevel();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_cleanup(int status);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_cleanup_fork();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_halt(int status);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_dlopen([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string file, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern IntPtr PL_dlerror();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_dlsym(void* handle, [NativeTypeName("char *")] sbyte* symbol);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_dlclose(void* handle);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_dispatch(int fd, int wait);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_add_to_protocol([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string buf, [NativeTypeName("size_t")] nuint count);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("char *")]
        public static extern sbyte* PL_prompt_string(int fd);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_write_prompt(int dowrite);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_prompt_next(int fd);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("char *")]
        public static extern sbyte* PL_atom_generator([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string prefix, int state);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("pl_wchar_t *")]
        public static extern uint* PL_atom_generator_w([NativeTypeName("const pl_wchar_t *")] uint* pref, [NativeTypeName("pl_wchar_t *")] uint* buffer, [NativeTypeName("size_t")] nuint buflen, int state);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_malloc([NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_malloc_atomic([NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_malloc_uncollectable([NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_malloc_atomic_uncollectable([NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_realloc(void* mem, [NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_malloc_unmanaged([NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_malloc_atomic_unmanaged([NativeTypeName("size_t")] nuint size);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_free(void* mem);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_linger(void* mem);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("PL_dispatch_hook_t")]
        public static extern delegate* unmanaged[Cdecl]<int, int> PL_dispatch_hook([NativeTypeName("PL_dispatch_hook_t")] delegate* unmanaged[Cdecl]<int, int> param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_abort_hook([NativeTypeName("PL_abort_hook_t")] delegate* unmanaged[Cdecl]<void> param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_initialise_hook([NativeTypeName("PL_initialise_hook_t")] delegate* unmanaged[Cdecl]<int, sbyte**, void> param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_abort_unhook([NativeTypeName("PL_abort_hook_t")] delegate* unmanaged[Cdecl]<void> param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("PL_agc_hook_t")]
        public static extern delegate* unmanaged[Cdecl]<nuint, int> PL_agc_hook([NativeTypeName("PL_agc_hook_t")] delegate* unmanaged[Cdecl]<nuint, int> param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_scan_options([NativeTypeName("term_t")] nuint options, int flags, [NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string opttype, [NativeTypeName("PL_option_t[]")] PL_option_t* specs, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("void (*)(int)")]
        public static extern delegate* unmanaged[Cdecl]<int, void> PL_signal(int sig, [NativeTypeName("void (*)(int)")] delegate* unmanaged[Cdecl]<int, void> func);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_sigaction(int sig, [NativeTypeName("pl_sigaction_t *")] pl_sigaction* act, [NativeTypeName("pl_sigaction_t *")] pl_sigaction* old);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_interrupt(int sig);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_raise(int sig);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_handle_signals();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_signum_ex([NativeTypeName("term_t")] nuint sig, int* n);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_action(int param0, __arglist);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_on_halt([NativeTypeName("int (*)(int, void *)")] delegate* unmanaged[Cdecl]<int, void*, int> param0, void* param1);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_exit_hook([NativeTypeName("int (*)(int, void *)")] delegate* unmanaged[Cdecl]<int, void*, int> param0, void* param1);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_backtrace(int depth, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("char *")]
        public static extern sbyte* PL_backtrace_string(int depth, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_check_data([NativeTypeName("term_t")] nuint data);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_check_stacks();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_current_prolog_flag([NativeTypeName("atom_t")] nuint name, int type, void* ptr);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("unsigned int")]
        public static extern uint PL_version_info(int which);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("intptr_t")]
        public static extern nint PL_query(int param0);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_thread_self();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_unify_thread_id([NativeTypeName("term_t")] nuint t, int i);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_thread_id_ex([NativeTypeName("term_t")] nuint t, int* idp);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_thread_alias(int tid, [NativeTypeName("atom_t *")] nuint* alias);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_thread_attach_engine(PL_thread_attr_t* attr);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_thread_destroy_engine();

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_thread_at_exit([NativeTypeName("void (*)(void *)")] delegate* unmanaged[Cdecl]<void*, void> function, void* closure, int global);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_thread_raise(int tid, int sig);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("PL_engine_t")]
        public static extern __PL_PL_local_data* PL_create_engine(PL_thread_attr_t* attributes);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_set_engine([NativeTypeName("PL_engine_t")] __PL_PL_local_data* engine, [NativeTypeName("PL_engine_t *")] __PL_PL_local_data** old);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_destroy_engine([NativeTypeName("PL_engine_t")] __PL_PL_local_data* engine);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("hash_table_t")]
        public static extern __PL_table* PL_new_hash_table(int size, [NativeTypeName("void (*)(table_key_t, table_value_t)")] delegate* unmanaged[Cdecl]<void*, void*, void> free_symbol);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_free_hash_table([NativeTypeName("hash_table_t")] __PL_table* table);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("table_value_t")]
        public static extern void* PL_lookup_hash_table([NativeTypeName("hash_table_t")] __PL_table* table, [NativeTypeName("table_key_t")] void* key);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("table_value_t")]
        public static extern void* PL_add_hash_table([NativeTypeName("hash_table_t")] __PL_table* table, [NativeTypeName("table_key_t")] void* key, [NativeTypeName("table_value_t")] void* value, int flags);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("table_value_t")]
        public static extern void* PL_del_hash_table([NativeTypeName("hash_table_t")] __PL_table* table, [NativeTypeName("table_key_t")] void* key);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_clear_hash_table([NativeTypeName("hash_table_t")] __PL_table* table);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        [return: NativeTypeName("hash_table_enum_t")]
        public static extern __PL_table_enum* PL_new_hash_table_enum([NativeTypeName("hash_table_t")] __PL_table* table);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_free_hash_table_enum([NativeTypeName("hash_table_enum_t")] __PL_table_enum* e);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_advance_hash_table_enum([NativeTypeName("hash_table_enum_t")] __PL_table_enum* e, [NativeTypeName("table_key_t *")] void** key, [NativeTypeName("table_value_t *")] void** value);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_register_profile_type(PL_prof_type_t* type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void* PL_prof_call(void* handle, PL_prof_type_t* type);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern void PL_prof_exit(void* node);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_prolog_debug([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string topic);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_prolog_nodebug([NativeTypeName("const char *")] [MarshalAs(UnmanagedType.LPStr)] string topic);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int _PL_get_xpce_reference([NativeTypeName("term_t")] nuint t, xpceref_t* @ref);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int _PL_unify_xpce_reference([NativeTypeName("term_t")] nuint t, xpceref_t* @ref);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int _PL_put_xpce_reference_i([NativeTypeName("term_t")] nuint t, [NativeTypeName("uintptr_t")] nuint r);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int _PL_put_xpce_reference_a([NativeTypeName("term_t")] nuint t, [NativeTypeName("atom_t")] nuint name);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_get_context([NativeTypeName("struct pl_context_t *")] pl_context_t* c, int thead_id);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_step_context([NativeTypeName("struct pl_context_t *")] pl_context_t* c);

        [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi,  ExactSpelling = true)]
        public static extern int PL_describe_context([NativeTypeName("struct pl_context_t *")] pl_context_t* c, [NativeTypeName("char *")] sbyte* buf, [NativeTypeName("size_t")] nuint len);
    }
}
