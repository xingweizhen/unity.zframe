--
-- @file 	framework/console/help.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date	2016-05-12 14:56:32
-- @desc    描述
--

return {
    __index = function (t, n)
        print("------------")
        for k,v in pairs(t) do
        	if k:sub(1, 2) ~= "__" then
            	print(k, type(v))
            end
        end
        print("------------")
    end
}

