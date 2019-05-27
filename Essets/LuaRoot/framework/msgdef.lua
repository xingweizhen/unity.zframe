--
-- @file    framework/msgdef.lua
-- @author  xing weizhen (xingweizhen@me-game.com)
-- @date    2019-05-24 16:28:12
-- @desc    描述
--

local P = { }

-- Requests --
local REQ_ID2Name, REQ_Name2ID = {}, {}
function P.add_req_msg(name, id)
	REQ_ID2Name[id] = name
	REQ_Name2ID[name] = id
end
function P.get_req_name(id) return REQ_ID2Name[id] end
function P.get_req_type(name) return REQ_Name2ID[name] end

-- respones --
local RSP_ID2Name, RSP_Name2ID = {}, {}
function P.add_rsp_msg(name, id)
	RSP_ID2Name[id] = name
	RSP_Name2ID[name] = id
end
function P.get_rsp_name(id) return RSP_ID2Name[id] end
function P.get_rsp_type(name) return RSP_Name2ID[name] end

return P
