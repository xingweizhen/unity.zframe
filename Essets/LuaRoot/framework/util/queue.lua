local ipairs, pairs, setmetatable, table
    = ipairs, pairs, setmetatable, table

local OBJDEF = { }
OBJDEF.__index = OBJDEF
OBJDEF.__tostring = function (self)
	return string.format("[Queue:n=%d]", #self)
end

function OBJDEF.new()
   	local self = { }
   	return setmetatable(self, OBJDEF)
end

function OBJDEF:enqueue(data)
    table.insert(self, data)
    return data
end

function OBJDEF:dequeue()
	return table.remove(self, 1)
end

function OBJDEF:peek()
	return self[1]
end

function OBJDEF:count()
	return #self
end

function OBJDEF:clear()
    for i=#self,1,-1 do
        table.remove(self, i)
    end
end

_G.DEF.Queue = OBJDEF
