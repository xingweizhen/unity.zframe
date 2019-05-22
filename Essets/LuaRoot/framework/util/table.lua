--
-- @file    framework/util/table.lua
-- @author  xing weizhen (kokohna@163.com)
-- @date    2016-02-29 11:11:58
-- @desc    描述
--

unpack = table.unpack

function table.void(T)
    if T == nil then
        return true
    else
        local k, v = next(T)
        return v == nil
    end
end

function table.hollow(T)
    if T == nil then return true end
    for _,v in pairs(T) do
        if type(v) == "table" then
            if not table.hollow(v) then return false end
        else
            return false
        end
    end
    return true
end

function table.clear(T)
    for k,v in pairs(T) do
        T[k] = nil
    end
end

-- 计算表中的元素数量
function table.count(T)
    local n = 0
    if T then
        for k,v in pairs(T) do
            n = n + 1
        end
    end
    return n
end

-- 获取键值为key的表，如果没有就创建一个
function table.need(T, key)
    local Sub = T[key]
    if Sub == nil then Sub = {}; T[key] = Sub end
    return Sub
end

-- 取走键值为key的值
function table.take(T, key)
    local Sub = rawget(T, key)
    rawset(T, key, nil)
    return Sub
end

function table.toarray(T, sort)
    local Array = {}
    for _,v in pairs(T) do
        table.insert(Array, v)
    end
    if sort then table.sort(Array, sort) end
    return Array
end

-- 根据key数组提取子表成为一个新的数组
function table.subarray(T, KeyArray, sortfunc)
    local Sub = {}
    for i,v in ipairs(KeyArray) do
        local S = T[v]
        if S then table.insert(Sub, S) end
    end
    if sortfunc then table.sort(Sub, sortfunc) end
    return Sub
end

-- key和value互换
function table.swapkv(T)
    local N = {}
    for k,v in pairs(T) do
        N[v] = k
    end
    return N
end

function table.tomap(Array, key)
    local Map = {}
    for _,v in ipairs(Array) do
        Map[v[key]] = v
    end
    return Map
end

function table.arrvalue(Arr)
    local Dst = {}
    for _,v in pairs(Arr) do
        Dst[v] = true
    end
    return Dst
end

-- 表深拷贝
function table.dup(orig, metable)
    local orig_type = type(orig)
    local copy
    if orig_type == 'table' then
        copy = {}
        for orig_key, orig_value in next, orig, nil do
            copy[table.dup(orig_key)] = table.dup(orig_value)
        end
        -- 可选拷贝元表
        if metable then setmetatable(copy, table.dup(getmetatable(orig))) end
    else
        -- number, string, boolean, etc
        copy = orig
    end
    return copy
end

function table.match(T, Sub, multiple)
    local Ents = multiple and {} or nil
    for _,Ent in pairs(T) do
        local found = true
        for k,v in pairs(Sub) do
            if Ent[k] ~= v then
                found = false
                break
            end
        end
        if found then
            if not multiple then
                return Ent
            else
                table.insert(Ents, Ent)
            end
        end
    end
    if Ents == nil or #Ents == 0 then return nil else return Ents end
end

function table.ifind(Array, value)
    if Array then
        for i,v in ipairs(Array) do
            if v == value then return i, v end
        end
    end
end

function table.find(Dict, value)
    if Dict then
        for k,v in pairs(Dict) do
            if v == value then return k, v end
        end
    end
end

-- 根据整数位值生成一个布尔数组
function table.num2bools(numb)
    local Array = {}
    while numb > 0 do
        local k = numb % 2
        numb = math.floor(numb / 2)
        table.insert(Array, k == 1)
    end
    return Array
end

function table.insert_once(Array, elm)
    for _,v in ipairs(Array) do
        if v == elm then return end
    end
    table.insert(Array, elm)
end

function table.remove_elm(Array, elm)
    if not Array then return end

    local i, _ = table.ifind(Array, elm)
    if i then table.remove(Array, i) end
end

function table.get_base_data(this)
    local Base = this.Base
    if Base == nil then
        local Cfg = _G.config(this.LIB)
        if Cfg and Cfg.get_dat then
            if this.dat then
                Base = Cfg.get_dat(this.dat)
                this.Base = Base
                if Base == nil then
                    libunity.LogW("{0}获取配置数据失败：数据不存在", tostring(this))
                end
            else
                libunity.LogW("{0}获取配置数据失败：'.dat'字段不存在", tostring(this))
            end
        else
            libunity.LogW("{0}获取配置数据失败：{1}不存在或者配置表未定义'get_dat'方法。",
                tostring(this), this.LIB)
        end
    end
    return Base
end

function table.merge(t1, t2)
    local newTable = {}
    for k,v in pairs(t1) do
        newTable[k] = v
    end
    for k,v in pairs(t2) do
        newTable[k] = v
    end
    return newTable
end

function table.print(table, prettyPrint)
    local jsTable = cjson.encode(table, prettyPrint)
    _G.print(tostring(table).."="..jsTable)
end
