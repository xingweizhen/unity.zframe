
#ifndef EXPORTFILE_H_
#define EXPORTFILE_H_

#   if defined (WIN32)

#   if defined( PARSEFILE_EXPORTS )
#         if defined (__cplusplus)
#              define SM_EXPORTS   extern "C"  __declspec( dllexport )
#         else
#			   define SM_EXPORTS   __declspec( dllexport )
#         endif
#   else
#         if defined (__cplusplus)
#				define SM_EXPORTS   extern "C" __declspec( dllimport )
#         else
#               define SM_EXPORTS   __declspec( dllimport )
#         endif
#   endif

#   else

#         if defined (__cplusplus)
#				define SM_EXPORTS   extern "C"
#         else
#               define SM_EXPORTS
#         endif

#   endif
const int ACTIVATION = 0X1FD508;
#endif
