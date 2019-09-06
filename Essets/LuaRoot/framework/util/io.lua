--
-- @file    framework/util/io.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2019-09-05 16:01:10
-- @desc    扩展IO库
--

local io = io

function io.copy(src, dst)
	local fsrc = io.open(src, "rb")
	if fsrc then
		local fdst = io.open(dst, "wb")
		if fdst then
			fdst:write(fsrc:read("*a"))
			fdst:flush()
			fdst:close()
		else
			error("io.copy failure: can't open destina file `" .. dst .. "`", 2)
		end
		fsrc:close()
	else
		error("io.copy failure: can't open source file `" .. src .. "`", 2)
	end
end
