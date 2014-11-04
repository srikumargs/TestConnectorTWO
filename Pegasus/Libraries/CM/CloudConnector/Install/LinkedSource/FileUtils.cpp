#include "StdAfx.h"
#include "FileUtils.h"
#include <shlobj.h>
#include <shlwapi.h>

BOOL DeleteDirectoryRecursive(std::ofstream_t& oLogStream, LPTSTR tszDirectory)
{
	BOOL            bSuccess                   = true;
	TCHAR           tszDestPath[MAX_PATH + 1]  = {0};
	WIN32_FIND_DATA findFileData               = {0};
	HANDLE          hFind                      = INVALID_HANDLE_VALUE;
	
	::PathCombine(tszDestPath, tszDirectory, _T("*"));
	hFind = FindFirstFile(tszDestPath, &findFileData);

	if (hFind == INVALID_HANDLE_VALUE)
	{
		oLogStream << _T("In DeleteDirectoryRecursive FindFirstFile failed; result is: ") << GetLastError() << _T(".") << std::endl;
		return false;
	}

	while(bSuccess)
	{
		if ( _tcscmp(findFileData.cFileName,_T(".")) == 0 || _tcscmp(findFileData.cFileName,_T("..")) == 0 )
		{
			// do nothing
		}
		else
		{		
			::PathCombine(tszDestPath, tszDirectory,findFileData.cFileName);

			if ((findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
			{
				// if it's a directory... recurse down and delete files and folders.
				bSuccess = DeleteDirectoryRecursive( oLogStream, tszDestPath );
			}
			else
			{
				// it's a file... delete it
				if ( findFileData.dwFileAttributes & FILE_ATTRIBUTE_READONLY )
					::SetFileAttributes( tszDestPath, (findFileData.dwFileAttributes & ~FILE_ATTRIBUTE_READONLY));

				bSuccess = DeleteFile( oLogStream, tszDestPath );
			}
		}

		// find next file
		if (bSuccess && ::FindNextFile(hFind, &findFileData) == FALSE)
		{
			DWORD err = ::GetLastError();
			if (err  == ERROR_NO_MORE_FILES)
				break;
			else
			{
				// we got an unknown error.. bail out.
				bSuccess = false;
				oLogStream << _T("FindNextFile in '") << tszDirectory << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
			}
		}
	}

	::FindClose(hFind);

	if ( bSuccess )
		bSuccess = DeleteDirectory(oLogStream, tszDirectory);

	return bSuccess;
}

void DeleteFiles(std::ofstream_t& oLogStream, const std::list<std::string_t>& fileNames)
{
	for(std::list<std::string_t>::const_iterator i = fileNames.begin() ; i != fileNames.end() ; i++)
	{
		DeleteFile(oLogStream, const_cast<LPTSTR>((*i).c_str()));
	}
}

BOOL DeleteFile(std::ofstream_t& oLogStream, LPTSTR tszFile)
{
	BOOL bSuccess = TRUE;
	if(!::DeleteFile(tszFile))
	{
		bSuccess = FALSE;
		oLogStream << _T("DeleteFile '") << tszFile << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
	}
	else
	{
		oLogStream << _T("DeleteFile '") << tszFile << _T("' succeeded.") << std::endl;
	}
	return bSuccess;
}

BOOL DeleteDirectory(std::ofstream_t& oLogStream, LPCTSTR tszDir)
{
	BOOL bSuccess = TRUE;
	if(!::RemoveDirectory(tszDir))
	{
		bSuccess = FALSE;
		oLogStream << _T("DeleteDirectory '") << tszDir << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
	}
	else
	{
		oLogStream << _T("DeleteDirectory '") << tszDir << _T("' succeeded.") << std::endl;
	}
	return bSuccess;
}

std::list<std::string_t> FindFiles(std::ofstream_t& oLogStream, LPCTSTR lpctszPath, LPCTSTR lpctszFileSpec)
{
	std::list<std::string_t> result;

	TCHAR tszDestPath[MAX_PATH + 1];
	::PathCombine(tszDestPath, lpctszPath, lpctszFileSpec);

	WIN32_FIND_DATA findFileData;
	HANDLE hFind = INVALID_HANDLE_VALUE;
	hFind = FindFirstFile(tszDestPath, &findFileData);
	if(hFind == INVALID_HANDLE_VALUE) 
	{
		oLogStream << _T("FindFirstFile failed; result is: ") << GetLastError() << _T(".") << std::endl;
	} 
	else 
	{
		oLogStream << _T("First file is: '") << findFileData.cFileName << _T("'.") << std::endl;
		::PathCombine(tszDestPath, lpctszPath, findFileData.cFileName);
		if(0 != _tcscmp(_T("."), findFileData.cFileName) && 
			0 != _tcscmp(_T(".."), findFileData.cFileName))
		{
			result.push_back(tszDestPath);
		}
		while(FindNextFile(hFind, &findFileData) != 0) 
		{
			oLogStream << _T("Next file is: '") << findFileData.cFileName << _T("'.") << std::endl;
			::PathCombine(tszDestPath, lpctszPath, findFileData.cFileName);
			if(0 != _tcscmp(_T("."), findFileData.cFileName) && 
				0 != _tcscmp(_T(".."), findFileData.cFileName))
			{
				result.push_back(tszDestPath);
			}
		}

		DWORD dwLastError = GetLastError();
		if(dwLastError != ERROR_NO_MORE_FILES) 
		{
			oLogStream << _T("FindNextFile failed; result is: ") << dwLastError << _T(".") << std::endl;
		}

		FindClose(hFind);
	}

	return result;
}

void CopyFiles(std::ofstream_t& oLogStream, const std::list<std::string_t>& fileNames, const std::string_t& dPath)
{
	int result = ::SHCreateDirectoryEx (NULL, dPath.c_str(),NULL);
    oLogStream << _T("SHCreateDirectoryEx '") << dPath << _T("' completed; result is: ") << result << _T(".") << std::endl;


	for(std::list<std::string_t>::const_iterator i = fileNames.begin() ; i != fileNames.end() ; i++)
	{
		TCHAR tszDrive[_MAX_DRIVE];
		TCHAR tszDir[_MAX_DIR];
		TCHAR tszFilename[_MAX_FNAME];
		TCHAR tszExt[_MAX_EXT];
		_tsplitpath_s(const_cast<LPTSTR>((*i).c_str()), tszDrive, tszDir, tszFilename, tszExt); 
		std::string_t sDestPath = dPath + _T("\\") + tszFilename + tszExt;
		SageCopyFile(oLogStream, (*i).c_str(), sDestPath.c_str());
	}
}

BOOL SageCopyFile(std::ofstream_t& oLogStream, LPCTSTR tszSourceFile, LPCTSTR tszDestFile)
{
	BOOL bSuccess = TRUE;
	if(!::CopyFile(tszSourceFile, tszDestFile, FALSE))
	{
		bSuccess = FALSE;
		oLogStream << _T("CopyFile '") << tszSourceFile << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
	}
	else
	{
		oLogStream << _T("CopyFile '") << tszSourceFile << _T("' succeeded.") << std::endl;
	}
	return bSuccess;
}

BOOL MoveFileReplaceExisting(std::ofstream_t& oLogStream, LPTSTR tszSourceFile, LPTSTR tszDestFile)
{
	BOOL bSuccess = TRUE;
	if(!::MoveFileEx(tszSourceFile, tszDestFile, MOVEFILE_REPLACE_EXISTING))
	{
		bSuccess = FALSE;
		oLogStream << _T("MoveFileEx '") << tszSourceFile << _T("' to '") << tszDestFile << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
	}
	else
	{
		oLogStream << _T("MoveFileEx '") << tszSourceFile << _T("' to '") << tszDestFile << _T("' succeeded.") << std::endl;
	}
	return bSuccess;
}

BOOL DirectoryExists(std::ofstream_t& oLogStream, LPCTSTR szPath)
{
  DWORD dwAttrib = GetFileAttributes(szPath);
  BOOL bResult = (dwAttrib != INVALID_FILE_ATTRIBUTES && (dwAttrib & FILE_ATTRIBUTE_DIRECTORY));

  oLogStream << _T("DirectoryExists '") << szPath << _T("' result is: ") << bResult << _T(".") << std::endl;

  return bResult;
}

BOOL FileExists(std::ofstream_t& oLogStream, LPCTSTR szPath)
{
  DWORD dwAttrib = GetFileAttributes(szPath);
  BOOL bResult = (dwAttrib != INVALID_FILE_ATTRIBUTES && !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY));
 
  oLogStream << _T("FileExists '") << szPath << _T("' result is: ") << bResult << _T(".") << std::endl;

  return bResult;
}

BOOL MoveDirectoryRecursive(std::ofstream_t& oLogStream, LPCTSTR szOldDir, LPCTSTR szNewDir)
{
	BOOL bResult = FALSE;

	if(DirectoryExists(oLogStream, szOldDir))
	{
		if(!DirectoryExists(oLogStream, szNewDir))
		{
			if(::MoveFile(szOldDir, szNewDir))
			{
				bResult = TRUE;
				oLogStream << _T("MoveFile '") << szOldDir << _T("' to '") << szNewDir << _T("' succeeded.") << std::endl;
			}
			else
			{
				oLogStream << _T("MoveFile '") << szOldDir << _T("' to '") << szNewDir << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
			}
		}
		else
		{
			BOOL            bSuccess                   = true;
			TCHAR           tszSourcePath[MAX_PATH + 1]  = {0};
			TCHAR           tszDestPath[MAX_PATH + 1]  = {0};
			WIN32_FIND_DATA findFileData               = {0};
			HANDLE          hFind                      = INVALID_HANDLE_VALUE;
			
			::PathCombine(tszDestPath, szOldDir, _T("*"));
			hFind = FindFirstFile(tszDestPath, &findFileData);
			if (hFind == INVALID_HANDLE_VALUE)
			{
				oLogStream << _T("In DeleteDirectoryRecursive FindFirstFile failed; result is: ") << GetLastError() << _T(".") << std::endl;				
				return false;
			}
			
			while(bSuccess)
			{
				if ( _tcscmp(findFileData.cFileName,_T(".")) == 0 || _tcscmp(findFileData.cFileName,_T("..")) == 0 )
				{
					// do nothing
				}
				else
				{		
					::PathCombine(tszSourcePath, szOldDir, findFileData.cFileName);
					::PathCombine(tszDestPath, szNewDir, findFileData.cFileName);
					if ((findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY))
					{
						// if it's a directory... recurse down and move files and folders.
						bSuccess = MoveDirectoryRecursive(oLogStream, tszSourcePath, tszDestPath);
					}
					else
					{
						// it's a file... move it
						bSuccess = MoveFileReplaceExisting(oLogStream, tszSourcePath, tszDestPath);
					}
				}
				
				// find next file
				if (bSuccess && ::FindNextFile(hFind, &findFileData) == FALSE)
				{
					DWORD err = ::GetLastError();
					if (err  == ERROR_NO_MORE_FILES)
						break;
					else
					{
						// we got an unknown error.. bail out.
						bSuccess = false;
						oLogStream << _T("FindNextFile in '") << szOldDir << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
					}
				}
			}
			
			::FindClose(hFind);
			
			if ( bSuccess )
				bSuccess = DeleteDirectory(oLogStream, szOldDir);
		}
	}
	else
	{
    	oLogStream << _T("MoveDirectory '") << szOldDir << _T("' to '") << szNewDir << _T("' failed because szOldDir does not exist.") << std::endl;
	}
	
	return bResult;
}