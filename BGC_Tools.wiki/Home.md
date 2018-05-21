# Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public The MIT `[`License`](#_l_i_c_e_n_s_e_8txt_1a8df284531fa4e8c4832111d161b918e2)`(MIT)`            | 
`public The MIT free of to any person obtaining a `[`copy`](#_l_i_c_e_n_s_e_8txt_1aff1d4c6b756ebf691fa44a0904f68658)` of this software and associated documentation `[`files`](#_l_i_c_e_n_s_e_8txt_1abdb4f4971cf029244bb81834ee9b393d)`(the"Software")`            | 
`public function `[`toggleVisibility`](#dynsections_8js_1a1922c462474df7dfd18741c961d59a25)`(linkObj)`            | 
`public function `[`updateStripes`](#dynsections_8js_1a8f7493ad859d4fbf2523917511ee7177)`()`            | 
`public function `[`toggleLevel`](#dynsections_8js_1a19f577cc1ba571396a85bb1f48bf4df2)`(level)`            | 
`public function `[`toggleFolder`](#dynsections_8js_1af244da4527af2d845dca04f5656376cd)`(id)`            | 
`public function `[`toggleInherit`](#dynsections_8js_1ac057b640b17ff32af11ced151c9305b4)`(id)`            | 
`public `[`b`](#jquery_8js_1a2fa551895933fae935a0a6b87282241d)` `[`extend`](#jquery_8js_1a5fb206c91c64d1be35fde236706eab86)`({cssHooks:{opacity:{get:function(bw, bv){`[`if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bv){var e=[Z](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)(bw,"opacity","opacity");return e===""?"1":e}else{return bw.style.opacity}}}}, cssNumber:{fillOpacity:true, fontWeight:true, lineHeight:true, opacity:true, orphans:true, widows:true, zIndex:true, zoom:true}, cssProps:{"float":b.support.cssFloat?"cssFloat":"styleFloat"}, style:function(bx, bw, bD, by){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(!bx\|\|bx.nodeType===3\|\|bx.nodeType===8\|\|!bx.style){return}var bB, bC, bz=b.camelCase(bw), bv=bx.style, bE=b.cssHooks[bz];bw=b.cssProps[bz]\|\|bz;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bD!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){bC=typeof bD;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bC==="string"&&(bB=I.exec(bD))){bD=(+(bB[1]+1)*+bB[2])+parseFloat([b.css](#jquery_8js_1a89ad527fcd82c01ebb587332f5b4fcd4)(bx, bw));bC="number"}if(bD==null\|\|bC==="number"&&isNaN(bD)){return}[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bC==="number"&&!b.cssNumber[bz]){bD+="px"}if(!bE\|\|!("set"in bE)\|\|(bD=bE.set(bx, bD))!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){try{bv[bw]=bD}catch(bA){}}}else{[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bE &&"get"in bE &&(bB=bE.get(bx, false, by))!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){return bB}return bv[bw]}}, css:function(by, bx, bv){var bw, e;bx=b.camelCase(bx);e=b.cssHooks[bx];bx=b.cssProps[bx]\|\|bx;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bx==="cssFloat"){bx="float"}if(e &&"get"in e &&(bw=e.get(by, true, bv))!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){return bw}else{[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)([Z](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)){return [Z`](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)(by, bx)}}}, swap:function(bx, bw, by){var e={};for(var bv in bw){e[bv]=bx.style[bv];bx.style[bv]=bw[bv]}by.call(bx);for(bv in bw)`{bx.style[bv]=e[bv]}}})`            | 
`public `[`b`](#jquery_8js_1a2fa551895933fae935a0a6b87282241d)` `[`each`](#jquery_8js_1a871ff39db627c54c710a3e9909b8234c)`(function(bv, e){b.cssHooks`[`e]={get:function(by, bx, bw){var bz;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bx){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(by.offsetWidth!==0){return [p](#jquery_8js_1a2335e57f79b6acfb6de59c235dc8a83e)(by, e, bw)}else{b.swap(by, a7, function(){bz=[p](#jquery_8js_1a2335e57f79b6acfb6de59c235dc8a83e)(by, e, bw)})}return bz}}, set:function(bw, bx){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bc.test(bx)){bx=parseFloat(bx);[if`](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bx >=0)`{return bx+"px"}}else{return bx}}}})`            | 
`public  `[`if`](#jquery_8js_1a9db6d45a025ad692282fe23e69eeba43)`(!b.support. opacity)`            | 
`public  `[`b`](#jquery_8js_1a2fa551895933fae935a0a6b87282241d)`(function(){`[`if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(!b.support.reliableMarginRight){b.cssHooks.marginRight={get:function(bw, bv){var e;b.swap(bw,{display:"inline-block"}, function(){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bv){e=[Z`](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)(bw,"margin-right","marginRight")}else{e=bw.style.marginRight}})`;return e}}}})`            | 
`public  `[`if`](#jquery_8js_1a30d3d2cd5b567c9f31b2aa30b9cb3bb8)`(av.defaultView &&av.defaultView. getComputedStyle)`            | 
`public  `[`if`](#jquery_8js_1a2c54bd8ed7482e89d19331ba61fe221c)`(av.documentElement. currentStyle)`            | 
`public function `[`p`](#jquery_8js_1a2335e57f79b6acfb6de59c235dc8a83e)`(by,bw,bv)`            | 
`public  `[`if`](#jquery_8js_1a42cbfadee2b4749e8f699ea8d745a0e4)`(b.expr &&b.expr. filters)`            | 
`public function `[`convertToId`](#search_8js_1a196a29bd5a5ee7cd5b485e0753a49e57)`(search)`            | 
`public function `[`getXPos`](#search_8js_1a76d24aea0009f892f8ccc31d941c0a2b)`(item)`            | 
`public function `[`getYPos`](#search_8js_1a8d7b405228661d7b6216b6925d2b8a69)`(item)`            | 
`public function `[`SearchBox`](#search_8js_1a52066106482f8136aa9e0ec859e8188f)`(name,resultsPath,inFrame,label)`            | 
`public function `[`SearchResults`](#search_8js_1a9189b9f7a32b6bc78240f40348f7fe03)`(name)`            | 
`public function `[`setKeyActions`](#search_8js_1a98192fa2929bb8e4b0a890a4909ab9b2)`(elem,action)`            | 
`public function `[`setClassAttr`](#search_8js_1a499422fc054a5278ae32801ec0082c56)`(elem,attr)`            | 
`public function `[`createResults`](#search_8js_1a6b2c651120de3ed1dcf0d85341d51895)`()`            | 
`public function `[`init_search`](#search_8js_1ae95ec7d5d450d0a8d6928a594798aaf4)`()`            | 
`public  `[`if`](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)`(! window)`            | 
`public function `[`show`](#svgpan_8js_1aa8d9f2083cafa2af98efafed94901079)`()`            | Show the graph in the middle of the view, scaled to fit
`public function `[`init`](#svgpan_8js_1a898016c74bec720a57cce798a2ed4ee2)`(evt)`            | Register handlers
`namespace `[`BGC::DataStructures::Generic`](#namespace_b_g_c_1_1_data_structures_1_1_generic) | 
`namespace `[`BGC::Extensions`](#namespace_b_g_c_1_1_extensions) | 
`namespace `[`BGC::IO`](#namespace_b_g_c_1_1_i_o) | 
`namespace `[`BGC::IO::Logging`](#namespace_b_g_c_1_1_i_o_1_1_logging) | 
`namespace `[`BGC::MonoUtility`](#namespace_b_g_c_1_1_mono_utility) | 
`namespace `[`BGC::UI`](#namespace_b_g_c_1_1_u_i) | 
`namespace `[`BGC::Utility`](#namespace_b_g_c_1_1_utility) | 
`namespace `[`BGC::Utility::Inspector`](#namespace_b_g_c_1_1_utility_1_1_inspector) | 
`namespace `[`BGC::Utility::Math`](#namespace_b_g_c_1_1_utility_1_1_math) | 
`namespace `[`BGC::Web`](#namespace_b_g_c_1_1_web) | 
`namespace `[`BGC::Web::Utility`](#namespace_b_g_c_1_1_web_1_1_utility) | 
`namespace `[`LightJson`](#namespace_light_json) | 
`namespace `[`LightJson::Serialization`](#namespace_light_json_1_1_serialization) | 
`class `[`BGC::Web::AWSServer::BodyKeys`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_body_keys) | 
`class `[`Exception`](#class_exception) | 
`class `[`BGC::Web::AWSServer::HeaderKeys`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_header_keys) | 
`class `[`ICollection`](#class_i_collection) | 
`class `[`IDisposable`](#class_i_disposable) | 
`class `[`IEnumerable`](#class_i_enumerable) | 
`class `[`IEnumerable< KeyValuePair< string, JsonValue >>`](#class_i_enumerable_3_01_key_value_pair_3_01string_00_01_json_value_01_4_4) | 
`class `[`IEnumerator`](#class_i_enumerator) | 
`class `[`LightJson::JsonArray::JsonArrayDebugView`](#class_light_json_1_1_json_array_1_1_json_array_debug_view) | 
`class `[`LightJson::JsonObject::JsonObjectDebugView`](#class_light_json_1_1_json_object_1_1_json_object_debug_view) | 
`class `[`LightJson::JsonValue::JsonValueDebugView`](#class_light_json_1_1_json_value_1_1_json_value_debug_view) | 
`class `[`LightJson::JsonObject::JsonObjectDebugView::KeyValuePair`](#class_light_json_1_1_json_object_1_1_json_object_debug_view_1_1_key_value_pair) | 
`class `[`MonoBehaviour`](#class_mono_behaviour) | 
`class `[`PropertyAttribute`](#class_property_attribute) | 

## Members

#### `public The MIT `[`License`](#_l_i_c_e_n_s_e_8txt_1a8df284531fa4e8c4832111d161b918e2)`(MIT)` 

#### `public The MIT free of to any person obtaining a `[`copy`](#_l_i_c_e_n_s_e_8txt_1aff1d4c6b756ebf691fa44a0904f68658)` of this software and associated documentation `[`files`](#_l_i_c_e_n_s_e_8txt_1abdb4f4971cf029244bb81834ee9b393d)`(the"Software")` 

#### `public function `[`toggleVisibility`](#dynsections_8js_1a1922c462474df7dfd18741c961d59a25)`(linkObj)` 

#### `public function `[`updateStripes`](#dynsections_8js_1a8f7493ad859d4fbf2523917511ee7177)`()` 

#### `public function `[`toggleLevel`](#dynsections_8js_1a19f577cc1ba571396a85bb1f48bf4df2)`(level)` 

#### `public function `[`toggleFolder`](#dynsections_8js_1af244da4527af2d845dca04f5656376cd)`(id)` 

#### `public function `[`toggleInherit`](#dynsections_8js_1ac057b640b17ff32af11ced151c9305b4)`(id)` 

#### `public `[`b`](#jquery_8js_1a2fa551895933fae935a0a6b87282241d)` `[`extend`](#jquery_8js_1a5fb206c91c64d1be35fde236706eab86)`({cssHooks:{opacity:{get:function(bw, bv){`[`if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bv){var e=[Z](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)(bw,"opacity","opacity");return e===""?"1":e}else{return bw.style.opacity}}}}, cssNumber:{fillOpacity:true, fontWeight:true, lineHeight:true, opacity:true, orphans:true, widows:true, zIndex:true, zoom:true}, cssProps:{"float":b.support.cssFloat?"cssFloat":"styleFloat"}, style:function(bx, bw, bD, by){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(!bx||bx.nodeType===3||bx.nodeType===8||!bx.style){return}var bB, bC, bz=b.camelCase(bw), bv=bx.style, bE=b.cssHooks[bz];bw=b.cssProps[bz]||bz;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bD!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){bC=typeof bD;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bC==="string"&&(bB=I.exec(bD))){bD=(+(bB[1]+1)*+bB[2])+parseFloat([b.css](#jquery_8js_1a89ad527fcd82c01ebb587332f5b4fcd4)(bx, bw));bC="number"}if(bD==null||bC==="number"&&isNaN(bD)){return}[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bC==="number"&&!b.cssNumber[bz]){bD+="px"}if(!bE||!("set"in bE)||(bD=bE.set(bx, bD))!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){try{bv[bw]=bD}catch(bA){}}}else{[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bE &&"get"in bE &&(bB=bE.get(bx, false, by))!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){return bB}return bv[bw]}}, css:function(by, bx, bv){var bw, e;bx=b.camelCase(bx);e=b.cssHooks[bx];bx=b.cssProps[bx]||bx;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bx==="cssFloat"){bx="float"}if(e &&"get"in e &&(bw=e.get(by, true, bv))!==[L](#jquery_8js_1a38ee4c0b5f4fe2a18d0c783af540d253)){return bw}else{[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)([Z](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)){return [Z`](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)(by, bx)}}}, swap:function(bx, bw, by){var e={};for(var bv in bw){e[bv]=bx.style[bv];bx.style[bv]=bw[bv]}by.call(bx);for(bv in bw)`{bx.style[bv]=e[bv]}}})` 

#### `public `[`b`](#jquery_8js_1a2fa551895933fae935a0a6b87282241d)` `[`each`](#jquery_8js_1a871ff39db627c54c710a3e9909b8234c)`(function(bv, e){b.cssHooks`[`e]={get:function(by, bx, bw){var bz;[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bx){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(by.offsetWidth!==0){return [p](#jquery_8js_1a2335e57f79b6acfb6de59c235dc8a83e)(by, e, bw)}else{b.swap(by, a7, function(){bz=[p](#jquery_8js_1a2335e57f79b6acfb6de59c235dc8a83e)(by, e, bw)})}return bz}}, set:function(bw, bx){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bc.test(bx)){bx=parseFloat(bx);[if`](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bx >=0)`{return bx+"px"}}else{return bx}}}})` 

#### `public  `[`if`](#jquery_8js_1a9db6d45a025ad692282fe23e69eeba43)`(!b.support. opacity)` 

#### `public  `[`b`](#jquery_8js_1a2fa551895933fae935a0a6b87282241d)`(function(){`[`if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(!b.support.reliableMarginRight){b.cssHooks.marginRight={get:function(bw, bv){var e;b.swap(bw,{display:"inline-block"}, function(){[if](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)(bv){e=[Z`](#jquery_8js_1adc18d83abfd9f87d396e8fd6b6ac0fe1)(bw,"margin-right","marginRight")}else{e=bw.style.marginRight}})`;return e}}}})` 

#### `public  `[`if`](#jquery_8js_1a30d3d2cd5b567c9f31b2aa30b9cb3bb8)`(av.defaultView &&av.defaultView. getComputedStyle)` 

#### `public  `[`if`](#jquery_8js_1a2c54bd8ed7482e89d19331ba61fe221c)`(av.documentElement. currentStyle)` 

#### `public function `[`p`](#jquery_8js_1a2335e57f79b6acfb6de59c235dc8a83e)`(by,bw,bv)` 

#### `public  `[`if`](#jquery_8js_1a42cbfadee2b4749e8f699ea8d745a0e4)`(b.expr &&b.expr. filters)` 

#### `public function `[`convertToId`](#search_8js_1a196a29bd5a5ee7cd5b485e0753a49e57)`(search)` 

#### `public function `[`getXPos`](#search_8js_1a76d24aea0009f892f8ccc31d941c0a2b)`(item)` 

#### `public function `[`getYPos`](#search_8js_1a8d7b405228661d7b6216b6925d2b8a69)`(item)` 

#### `public function `[`SearchBox`](#search_8js_1a52066106482f8136aa9e0ec859e8188f)`(name,resultsPath,inFrame,label)` 

#### `public function `[`SearchResults`](#search_8js_1a9189b9f7a32b6bc78240f40348f7fe03)`(name)` 

#### `public function `[`setKeyActions`](#search_8js_1a98192fa2929bb8e4b0a890a4909ab9b2)`(elem,action)` 

#### `public function `[`setClassAttr`](#search_8js_1a499422fc054a5278ae32801ec0082c56)`(elem,attr)` 

#### `public function `[`createResults`](#search_8js_1a6b2c651120de3ed1dcf0d85341d51895)`()` 

#### `public function `[`init_search`](#search_8js_1ae95ec7d5d450d0a8d6928a594798aaf4)`()` 

#### `public  `[`if`](#svgpan_8js_1af5ea8bd8b0e968d48c903e56d0e3afd4)`(! window)` 

#### `public function `[`show`](#svgpan_8js_1aa8d9f2083cafa2af98efafed94901079)`()` 

Show the graph in the middle of the view, scaled to fit

#### `public function `[`init`](#svgpan_8js_1a898016c74bec720a57cce798a2ed4ee2)`(evt)` 

Register handlers

# namespace `BGC::DataStructures::Generic` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::DataStructures::Generic::DepletableBag`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag) | An unstable-sort set structure that acts as a select-without-replace bag.
`class `[`BGC::DataStructures::Generic::DepletableList`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list) | A depletable/refillable set with an underlying list and defined order.
`class `[`BGC::DataStructures::Generic::RingBuffer`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer) | Statically-sized ring buffer container.
`class `[`BGC::DataStructures::Generic::RingBufferEnum`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum) | [RingBuffer](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer) Enumerator class to enable proper list navigation.
`struct `[`BGC::DataStructures::Generic::Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node) | [Generic](#namespace_b_g_c_1_1_data_structures_1_1_generic)[Node](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node) for generating tree structure

# class `BGC::DataStructures::Generic::DepletableBag` 

```
class BGC::DataStructures::Generic::DepletableBag
  : public BGC::DataStructures::Generic::IDepletable< T >
```  

An unstable-sort set structure that acts as a select-without-replace bag.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`DepletableBag`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a0fb80d2729ddd4ec7fe535a8cf936d93)`()` | 
`public inline  `[`DepletableBag`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1aece2455c60dc6ed25f627a545814b8af)`(`[`IEnumerable`](#class_i_enumerable)`< T > values,bool autoRefill)` | 
`public inline T `[`PopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a538c9ac61edb58a86a59a505b899d424)`()` | Removes and returns the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable)
`public inline bool `[`TryPopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a266615a1ea0c824256b50d5a95fc275f)`(out T value)` | Removes the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable), if possible, and returns success
`public inline void `[`Reset`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a7a68ddb13f38ef242a3a25b33e89b7d5)`()` | Fills the bag back up.
`public inline bool `[`DepleteValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1acf49fae3f246c9398538f121667ac8a6)`(T value)` | Mark the first instance of value as depleted
`public inline bool `[`DepleteAllValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1afff7ddbfcc14bfdf70729f5e78cffc2c)`(T value)` | Mark all instances of value as depleted
`public inline bool `[`ContainsAnywhere`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1ad8833537e13a667a064e05428b6193c1)`(T value)` | Seaches active and depeleted items for value
`public inline IList< T > `[`GetAvailable`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1abc23d5cda5548142c9fd2f465c641f08)`()` | Returns a list of available items
`public inline void `[`CopyAllTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a8c18ae7f5067040c92ea4ba8dde35ff4)`(T[] array,int arrayIndex)` | Copies active and depleted values
`public inline void `[`Add`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a67de89d7f9e36103333ad06ccd5d9521)`(T value)` | 
`public inline void `[`Clear`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1af49a6485db88601f00d4a9bbf983722c)`()` | 
`public inline bool `[`Contains`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1aecb8256452754343679a716f14964f3d)`(T value)` | 
`public inline void `[`CopyTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a560a591b5563dc98eb09f819e5fc1bcc)`(T[] dest,int destIndex)` | 
`public inline bool `[`Remove`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1aed0041ebeb097e0280b6c851d239979d)`(T item)` | 
`public inline `[`IEnumerator`](#class_i_enumerator)`< T > `[`GetEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a21089391ec94779c0ae93ca36b1f4c59)`()` | 
`protected List< T > `[`values`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a0dbf26ad9db841cb8029feacf2112a8e) | 
`protected int `[`availableCount`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a40f8efa9d47a0b281f5df439f7d2c984) | 

## Members

#### `public inline  `[`DepletableBag`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a0fb80d2729ddd4ec7fe535a8cf936d93)`()` 

#### `public inline  `[`DepletableBag`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1aece2455c60dc6ed25f627a545814b8af)`(`[`IEnumerable`](#class_i_enumerable)`< T > values,bool autoRefill)` 

#### `public inline T `[`PopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a538c9ac61edb58a86a59a505b899d424)`()` 

Removes and returns the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable)

#### `public inline bool `[`TryPopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a266615a1ea0c824256b50d5a95fc275f)`(out T value)` 

Removes the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable), if possible, and returns success

#### `public inline void `[`Reset`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a7a68ddb13f38ef242a3a25b33e89b7d5)`()` 

Fills the bag back up.

#### `public inline bool `[`DepleteValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1acf49fae3f246c9398538f121667ac8a6)`(T value)` 

Mark the first instance of value as depleted

#### `public inline bool `[`DepleteAllValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1afff7ddbfcc14bfdf70729f5e78cffc2c)`(T value)` 

Mark all instances of value as depleted

#### `public inline bool `[`ContainsAnywhere`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1ad8833537e13a667a064e05428b6193c1)`(T value)` 

Seaches active and depeleted items for value

#### `public inline IList< T > `[`GetAvailable`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1abc23d5cda5548142c9fd2f465c641f08)`()` 

Returns a list of available items

#### `public inline void `[`CopyAllTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a8c18ae7f5067040c92ea4ba8dde35ff4)`(T[] array,int arrayIndex)` 

Copies active and depleted values

#### `public inline void `[`Add`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a67de89d7f9e36103333ad06ccd5d9521)`(T value)` 

#### `public inline void `[`Clear`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1af49a6485db88601f00d4a9bbf983722c)`()` 

#### `public inline bool `[`Contains`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1aecb8256452754343679a716f14964f3d)`(T value)` 

#### `public inline void `[`CopyTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a560a591b5563dc98eb09f819e5fc1bcc)`(T[] dest,int destIndex)` 

#### `public inline bool `[`Remove`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1aed0041ebeb097e0280b6c851d239979d)`(T item)` 

#### `public inline `[`IEnumerator`](#class_i_enumerator)`< T > `[`GetEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a21089391ec94779c0ae93ca36b1f4c59)`()` 

#### `protected List< T > `[`values`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a0dbf26ad9db841cb8029feacf2112a8e) 

#### `protected int `[`availableCount`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_bag_1a40f8efa9d47a0b281f5df439f7d2c984) 

# class `BGC::DataStructures::Generic::DepletableList` 

```
class BGC::DataStructures::Generic::DepletableList
  : public BGC::DataStructures::Generic::IDepletable< T >
```  

A depletable/refillable set with an underlying list and defined order.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`DepletableList`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4c367ac89f0fea68ba11140d207939e9)`()` | 
`public inline  `[`DepletableList`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a0a0c0b6f17fa536c63822e306c95bafe)`(`[`IEnumerable`](#class_i_enumerable)`< T > values,bool autoRefill)` | 
`public inline T `[`PopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a874aabe9a70c14eb60d841e888b31690)`()` | Removes and returns the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable)
`public inline bool `[`TryPopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1ad8e7a9cb02134e79ce2ed44926cf7b21)`(out T value)` | Removes the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable), if possible, and returns success
`public inline void `[`Reset`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a837ebb266fe5f18a3db7517f10556c7e)`()` | Fills the bag back up.
`public inline bool `[`DepleteValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4a6050ed7e27ee9c2b3d5884e6306d21)`(T value)` | Mark the first instance of value as depleted
`public inline bool `[`DepleteAllValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1ad6aafe7e060802f8abe2d7420db5ffa7)`(T value)` | Mark all instances of value as depleted
`public inline bool `[`ContainsAnywhere`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a59f4da7e67d3ef230d0a79fd09b5e791)`(T value)` | Seaches active and depeleted items for value
`public inline IList< T > `[`GetAvailable`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1abb828e1b69516b4fd894d0f65a4ade9c)`()` | Returns a list of available items
`public inline void `[`CopyAllTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a8fc1b92b7810ac0a2e8673f7285c22f2)`(T[] array,int arrayIndex)` | Copies active and depleted values
`public inline void `[`Add`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a3c9e0ac82398d5a1fc4916ced2498a3f)`(T value)` | 
`public inline void `[`Clear`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a75518e573378927236ec38a097119f47)`()` | 
`public inline bool `[`Contains`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1af20950661b0db33dd8c3bbc98c190e2e)`(T value)` | 
`public inline void `[`CopyTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4a963708660b6d8f6c64aae50873764b)`(T[] dest,int destIndex)` | 
`public inline bool `[`Remove`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a619adff84409fbd2acfa21ba48b815dc)`(T item)` | 
`public inline `[`IEnumerator`](#class_i_enumerator)`< T > `[`GetEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1af5e69cbace8ba059dc0f641e941f171c)`()` | 
`protected List< T > `[`values`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a9f6871b3fed8430f280f1403e64ad379) | 
`protected List< bool > `[`valueDepleted`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4440d079ee95f5d24ef65ef88a9adcb7) | 
`protected int `[`currentIndex`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a8c4fd04df84acbf5c3810825b07268a8) | 

## Members

#### `public inline  `[`DepletableList`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4c367ac89f0fea68ba11140d207939e9)`()` 

#### `public inline  `[`DepletableList`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a0a0c0b6f17fa536c63822e306c95bafe)`(`[`IEnumerable`](#class_i_enumerable)`< T > values,bool autoRefill)` 

#### `public inline T `[`PopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a874aabe9a70c14eb60d841e888b31690)`()` 

Removes and returns the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable)

#### `public inline bool `[`TryPopNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1ad8e7a9cb02134e79ce2ed44926cf7b21)`(out T value)` 

Removes the next value in the [IDepletable](#interface_b_g_c_1_1_data_structures_1_1_generic_1_1_i_depletable), if possible, and returns success

#### `public inline void `[`Reset`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a837ebb266fe5f18a3db7517f10556c7e)`()` 

Fills the bag back up.

#### `public inline bool `[`DepleteValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4a6050ed7e27ee9c2b3d5884e6306d21)`(T value)` 

Mark the first instance of value as depleted

#### `public inline bool `[`DepleteAllValue`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1ad6aafe7e060802f8abe2d7420db5ffa7)`(T value)` 

Mark all instances of value as depleted

#### `public inline bool `[`ContainsAnywhere`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a59f4da7e67d3ef230d0a79fd09b5e791)`(T value)` 

Seaches active and depeleted items for value

#### `public inline IList< T > `[`GetAvailable`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1abb828e1b69516b4fd894d0f65a4ade9c)`()` 

Returns a list of available items

#### `public inline void `[`CopyAllTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a8fc1b92b7810ac0a2e8673f7285c22f2)`(T[] array,int arrayIndex)` 

Copies active and depleted values

#### `public inline void `[`Add`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a3c9e0ac82398d5a1fc4916ced2498a3f)`(T value)` 

#### `public inline void `[`Clear`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a75518e573378927236ec38a097119f47)`()` 

#### `public inline bool `[`Contains`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1af20950661b0db33dd8c3bbc98c190e2e)`(T value)` 

#### `public inline void `[`CopyTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4a963708660b6d8f6c64aae50873764b)`(T[] dest,int destIndex)` 

#### `public inline bool `[`Remove`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a619adff84409fbd2acfa21ba48b815dc)`(T item)` 

#### `public inline `[`IEnumerator`](#class_i_enumerator)`< T > `[`GetEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1af5e69cbace8ba059dc0f641e941f171c)`()` 

#### `protected List< T > `[`values`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a9f6871b3fed8430f280f1403e64ad379) 

#### `protected List< bool > `[`valueDepleted`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a4440d079ee95f5d24ef65ef88a9adcb7) 

#### `protected int `[`currentIndex`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_depletable_list_1a8c4fd04df84acbf5c3810825b07268a8) 

# class `BGC::DataStructures::Generic::RingBuffer` 

```
class BGC::DataStructures::Generic::RingBuffer
  : public ICollection< T >
```  

Statically-sized ring buffer container.

#### Parameters
* `T`

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`RingBuffer`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a9da41afc28c2030a03b31bee89af2450)`(int bufferSize)` | Construct an empty ring buffer supporting bufferSize elements
`public inline  `[`RingBuffer`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ab1983252e354be69165b08b00761c485)`(`[`ICollection`](#class_i_collection)`< T > values,int bufferSize)` | Copy the list into a new buffer, optionally specify size.
`public inline void `[`Push`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a4ca1456bd2ee2c95bcfdc241ab56e1a8)`(T newValue)` | Add newValue to the end of the ringbuffer. Replaces the oldest member if at capacity.
`public inline void `[`Add`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ad52a2f74e4bef4b8e36453904e89a15e)`(T newValue)` | Add newValue to the end of the ringbuffer. Replaces the oldest member if at capacity.
`public inline void `[`Clear`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a6acd19d7890f35d8d83ad4ecd818cf3a)`()` | Clear the current items in the ring buffer. Doesn't resize or release buffer memory. Does release item handles.
`public inline bool `[`Contains`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a0b3c2f5d5e9e2a7d6d7ef55eed41a219)`(T value)` | Query the list for the argument value.
`public inline int `[`GetIndex`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a28db4f67f1ccad0719310d70e9258349)`(T value)` | Get the index of the argument value if it's present. Otherwise returns -1.
`public inline bool `[`Remove`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a4440ae854c6b2eb422d2818cd7e7a60b)`(T value)` | Removes the first element matching the argument value, if present, returns whether a value was removed.
`public inline void `[`RemoveAt`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a1599cf36708d9d4c7b679944f7f68f27)`(int index)` | Removes the item at index.
`public inline T `[`Pop`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a3503274ebfce37bf00735733725ec9f4)`()` | Removes and returns the item at the head (the newest).
`public inline T `[`PopBack`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a773f1edce239db1d8a1461d510388f83)`()` | Removes and returns the item at the tail (the oldest).
`public inline int `[`CountElement`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1adf4a8e37a0933f5ac375c9ee19316c1e)`(T value)` | Returns the number of elements whose value match the argument.
`public inline void `[`CopyTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ad2d63662375f7d2faa6cc71189173f45)`(T[] dest,int destIndex)` | Copy the list to the dest array, using the destIndex as an offset to the destination.
`public inline void `[`Resize`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ab2e6f7439571d449480d3efa63f94a83)`(int bufferSize)` | Resize the buffer of this list to support bufferSize elements.
`public inline `[`RingBufferEnum`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum)`< T > `[`GetRingEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a681b742d7e11fee5378a7bee41d64201)`()` | 
`public inline `[`IEnumerator`](#class_i_enumerator)`< T > `[`GetEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a76e38603e1106f9d532cc4f28b6a8256)`()` | 

## Members

#### `public inline  `[`RingBuffer`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a9da41afc28c2030a03b31bee89af2450)`(int bufferSize)` 

Construct an empty ring buffer supporting bufferSize elements

#### Parameters
* `bufferSize`

#### `public inline  `[`RingBuffer`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ab1983252e354be69165b08b00761c485)`(`[`ICollection`](#class_i_collection)`< T > values,int bufferSize)` 

Copy the list into a new buffer, optionally specify size.

#### Parameters
* `values` 
* `bufferSize`

#### `public inline void `[`Push`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a4ca1456bd2ee2c95bcfdc241ab56e1a8)`(T newValue)` 

Add newValue to the end of the ringbuffer. Replaces the oldest member if at capacity.

#### Parameters
* `newValue`

#### `public inline void `[`Add`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ad52a2f74e4bef4b8e36453904e89a15e)`(T newValue)` 

Add newValue to the end of the ringbuffer. Replaces the oldest member if at capacity.

#### Parameters
* `newValue`

#### `public inline void `[`Clear`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a6acd19d7890f35d8d83ad4ecd818cf3a)`()` 

Clear the current items in the ring buffer. Doesn't resize or release buffer memory. Does release item handles.

#### `public inline bool `[`Contains`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a0b3c2f5d5e9e2a7d6d7ef55eed41a219)`(T value)` 

Query the list for the argument value.

#### Parameters
* `value` 

#### Returns

#### `public inline int `[`GetIndex`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a28db4f67f1ccad0719310d70e9258349)`(T value)` 

Get the index of the argument value if it's present. Otherwise returns -1.

#### Parameters
* `value` 

#### Returns

#### `public inline bool `[`Remove`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a4440ae854c6b2eb422d2818cd7e7a60b)`(T value)` 

Removes the first element matching the argument value, if present, returns whether a value was removed.

#### Parameters
* `value` 

#### Returns

#### `public inline void `[`RemoveAt`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a1599cf36708d9d4c7b679944f7f68f27)`(int index)` 

Removes the item at index.

#### Parameters
* `index` 

#### Parameters
* `IndexOutOfRangeException` Throws System.IndexOutOfRangeException if the index exceeds the available count.

#### `public inline T `[`Pop`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a3503274ebfce37bf00735733725ec9f4)`()` 

Removes and returns the item at the head (the newest).

#### Returns

#### `public inline T `[`PopBack`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a773f1edce239db1d8a1461d510388f83)`()` 

Removes and returns the item at the tail (the oldest).

#### Returns

#### `public inline int `[`CountElement`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1adf4a8e37a0933f5ac375c9ee19316c1e)`(T value)` 

Returns the number of elements whose value match the argument.

#### Parameters
* `value` 

#### Returns

#### `public inline void `[`CopyTo`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ad2d63662375f7d2faa6cc71189173f45)`(T[] dest,int destIndex)` 

Copy the list to the dest array, using the destIndex as an offset to the destination.

#### Parameters
* `dest` 
* `destIndex`

#### `public inline void `[`Resize`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1ab2e6f7439571d449480d3efa63f94a83)`(int bufferSize)` 

Resize the buffer of this list to support bufferSize elements.

#### Parameters
* `bufferSize`

#### `public inline `[`RingBufferEnum`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum)`< T > `[`GetRingEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a681b742d7e11fee5378a7bee41d64201)`()` 

#### `public inline `[`IEnumerator`](#class_i_enumerator)`< T > `[`GetEnumerator`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_1a76e38603e1106f9d532cc4f28b6a8256)`()` 

# class `BGC::DataStructures::Generic::RingBufferEnum` 

```
class BGC::DataStructures::Generic::RingBufferEnum
  : public IEnumerator< T >
```  

[RingBuffer](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer) Enumerator class to enable proper list navigation.

#### Parameters
* `T`

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public T[] `[`values`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1aced3719f973c32a92dd47c99a314312c) | 
`public int `[`availableCount`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a39e54d8d230f009cb5be9b483e482e64) | 
`public int `[`headIndex`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a3570ac7440463ae0d11079205f9117d8) | 
`public inline  `[`RingBufferEnum`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1ab706f72efd0b06904803b1f207c34e95)`(T[] values,int availableCount,int headIndex)` | 
`public inline bool `[`MoveNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a3010ce95836593eb2e46ae096c58a01b)`()` | 
`public inline void `[`Reset`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a0523d09b74bc1671836b05f9614c003e)`()` | 

## Members

#### `public T[] `[`values`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1aced3719f973c32a92dd47c99a314312c) 

#### `public int `[`availableCount`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a39e54d8d230f009cb5be9b483e482e64) 

#### `public int `[`headIndex`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a3570ac7440463ae0d11079205f9117d8) 

#### `public inline  `[`RingBufferEnum`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1ab706f72efd0b06904803b1f207c34e95)`(T[] values,int availableCount,int headIndex)` 

#### `public inline bool `[`MoveNext`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a3010ce95836593eb2e46ae096c58a01b)`()` 

#### `public inline void `[`Reset`](#class_b_g_c_1_1_data_structures_1_1_generic_1_1_ring_buffer_enum_1a0523d09b74bc1671836b05f9614c003e)`()` 

# struct `BGC::DataStructures::Generic::Node` 

[Generic](#namespace_b_g_c_1_1_data_structures_1_1_generic)[Node](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node) for generating tree structure

> Todo: : adding some functions to make this easier to use would be ideal 

#### Parameters
* `T`

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public T `[`Value`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1ae14312160354d6aca38e1894ec2d28d4) | 
`public List< `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node)`< T > > `[`Children`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1ab1145124faa5fa1e52e4e4ae0572bf70) | 
`public inline  `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1a2e205517ed1fe59ef7578910e590afa7)`(T value)` | Construct a node with no children
`public inline  `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1a5f91d15b2b003718cf92d49cb426aedc)`(T value,List< `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node)`< T >> children)` | Construct a node with a list of children
`public inline  `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1adb2404ebfd2bd65714bbfeadf1e8e48e)`(T value,`[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node)`< T >[] children)` | Construct a node with an array of children

## Members

#### `public T `[`Value`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1ae14312160354d6aca38e1894ec2d28d4) 

#### `public List< `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node)`< T > > `[`Children`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1ab1145124faa5fa1e52e4e4ae0572bf70) 

#### `public inline  `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1a2e205517ed1fe59ef7578910e590afa7)`(T value)` 

Construct a node with no children

#### Parameters
* `value`

#### `public inline  `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1a5f91d15b2b003718cf92d49cb426aedc)`(T value,List< `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node)`< T >> children)` 

Construct a node with a list of children

#### Parameters
* `value` 
* `children`

#### `public inline  `[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node_1adb2404ebfd2bd65714bbfeadf1e8e48e)`(T value,`[`Node`](#struct_b_g_c_1_1_data_structures_1_1_generic_1_1_node)`< T >[] children)` 

Construct a node with an array of children

#### Parameters
* `value` 
* `children`

# namespace `BGC::Extensions` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::Extensions::ArrayExtensions`](#class_b_g_c_1_1_extensions_1_1_array_extensions) | 
`class `[`BGC::Extensions::JsonExtensions`](#class_b_g_c_1_1_extensions_1_1_json_extensions) | 
`class `[`BGC::Extensions::ListExtension`](#class_b_g_c_1_1_extensions_1_1_list_extension) | Set of extensions for a list for easier use of ILists
`class `[`BGC::Extensions::Vector2Extension`](#class_b_g_c_1_1_extensions_1_1_vector2_extension) | Set of extensions for unity data structure Vector2

# class `BGC::Extensions::ArrayExtensions` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Extensions::JsonExtensions` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Extensions::ListExtension` 

Set of extensions for a list for easier use of ILists

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Extensions::Vector2Extension` 

Set of extensions for unity data structure Vector2

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# namespace `BGC::IO` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::IO::DataManagement`](#class_b_g_c_1_1_i_o_1_1_data_management) | 
`class `[`BGC::IO::FileExtensions`](#class_b_g_c_1_1_i_o_1_1_file_extensions) | 
`class `[`BGC::IO::FilePath`](#class_b_g_c_1_1_i_o_1_1_file_path) | 
`class `[`BGC::IO::FileReader`](#class_b_g_c_1_1_i_o_1_1_file_reader) | 
`class `[`BGC::IO::FileWriter`](#class_b_g_c_1_1_i_o_1_1_file_writer) | 
`class `[`BGC::IO::LogDirectories`](#class_b_g_c_1_1_i_o_1_1_log_directories) | 
`class `[`BGC::IO::ParsingException`](#class_b_g_c_1_1_i_o_1_1_parsing_exception) | 

# class `BGC::IO::DataManagement` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::IO::FileExtensions` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public const string `[`JSON`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1ace98f935e29aedccf39b8153441c7264) | 
`public const string `[`CSV`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1a822d8f05b5b588d45a2b90236d2876b3) | 
`public const string `[`XML`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1a4297a3c71675197f30f54e432175dabb) | 
`public const string `[`BGC`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1abec966fc1936e2ad22be61407425888c) | 

## Members

#### `public const string `[`JSON`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1ace98f935e29aedccf39b8153441c7264) 

#### `public const string `[`CSV`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1a822d8f05b5b588d45a2b90236d2876b3) 

#### `public const string `[`XML`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1a4297a3c71675197f30f54e432175dabb) 

#### `public const string `[`BGC`](#class_b_g_c_1_1_i_o_1_1_file_extensions_1abec966fc1936e2ad22be61407425888c) 

# class `BGC::IO::FilePath` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::IO::FileReader` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::IO::FileWriter` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::IO::LogDirectories` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public const string `[`LogsDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a058fc982cd5afe02bbc12de81466f86d) | 
`public const string `[`S3StagingDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a307bc1ae66c4861edf087672b05b0bdf) | 
`public const string `[`ExceptionsDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a4de65b77b489fbef27bcc9a7c212baa5) | 
`public const string `[`S3PermanentDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a8d79c25b7072ea0a12f26600c27b003b) | 

## Members

#### `public const string `[`LogsDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a058fc982cd5afe02bbc12de81466f86d) 

#### `public const string `[`S3StagingDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a307bc1ae66c4861edf087672b05b0bdf) 

#### `public const string `[`ExceptionsDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a4de65b77b489fbef27bcc9a7c212baa5) 

#### `public const string `[`S3PermanentDirectory`](#class_b_g_c_1_1_i_o_1_1_log_directories_1a8d79c25b7072ea0a12f26600c27b003b) 

# class `BGC::IO::ParsingException` 

```
class BGC::IO::ParsingException
  : public Exception
```  

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public string `[`correctiveAction`](#class_b_g_c_1_1_i_o_1_1_parsing_exception_1aa93174bb22d95a6b5304dbaf3f9acbd3) | 
`public inline  `[`ParsingException`](#class_b_g_c_1_1_i_o_1_1_parsing_exception_1a25e959a6826174ef343b110d84162836)`(string message,string correctiveAction)` | 

## Members

#### `public string `[`correctiveAction`](#class_b_g_c_1_1_i_o_1_1_parsing_exception_1aa93174bb22d95a6b5304dbaf3f9acbd3) 

#### `public inline  `[`ParsingException`](#class_b_g_c_1_1_i_o_1_1_parsing_exception_1a25e959a6826174ef343b110d84162836)`(string message,string correctiveAction)` 

# namespace `BGC::IO::Logging` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::IO::Logging::Logger`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger) | 
`class `[`BGC::IO::Logging::LoggingKeys`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys) | 

# class `BGC::IO::Logging::Logger` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`Logger`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ad5ae29329c0f13e39a98dcc5edafdc95)`(`[`LogType`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ab03895c7cf8f5f594c231aa9513062d9)` type,string applicationName,string applicationVersion,string userName,int sessionNumber,int runNumber,string delimiter)` | 
`public inline void `[`PushLine`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a450f8ca3d659f68fdb2d7b1ac07ec630)`(params IConvertible[] strings)` | Push a line to the logger. a file
`public inline void `[`PushString`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a7de8cce7b2ce75b9aaf49bf10ed2f6ac)`(string str)` | Push a string to the logger. Will throw an exception if the logger hasn't been opened
`public inline void `[`CloseFile`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ad80e25518f6a0ba4af943695cfed4d95)`()` | 
`public inline void `[`CloseFile`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a10c1499e6315338e7e1194a50baf001e)`(string userName,string bucket,string serverPath)` | 
`protected const string `[`dateTimeFormat`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ac11f47be888b85c7ba1bd27c6c354b85) | 
`protected const string `[`bgcExtension`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ade142db0b9a30030077b3d1b5006da33) | 
`protected string `[`delimiter`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a3004e4c89ccf17420c790e9401a3bb6e) | 
`protected abstract `[`JsonObject`](#class_light_json_1_1_json_object)` `[`ConstructColumnMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1af43a9c04992c29f14686307c803c00fe)`()` | 
`protected abstract `[`JsonObject`](#class_light_json_1_1_json_object)` `[`ConstructValueMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ad4af09334bc0c1ca6d2af991a33083bd)`()` | 
`protected inline void `[`ApplyHeaders`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ac3c823ee08460e7df8a839a8e6eddda9)`()` | 
`protected inline string `[`GetNewLogName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1aa343bfd83dbffc1daa979f6c7a6a3be8)`(string userName,int runNumber,int session)` | 
`protected inline string `[`GetSummaryFileName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a725994630d9161f4d356646e23823a91)`(string userName,int sessionNumber)` | 
`protected inline string `[`GetExceptionFileName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a100a55da9153f0db61ef2f70d5ce100f)`()` | 
`protected inline string `[`PathForLogFile`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a8e3d2b4713248800d39bda7b2a782c7e)`(string userName,string filename)` | 
`protected inline void `[`ApplyRequiredFields`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a002e8058d833237a66cb935823b7af73)`(`[`JsonObject`](#class_light_json_1_1_json_object)` jo)` | 

## Members

#### `public inline  `[`Logger`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ad5ae29329c0f13e39a98dcc5edafdc95)`(`[`LogType`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ab03895c7cf8f5f594c231aa9513062d9)` type,string applicationName,string applicationVersion,string userName,int sessionNumber,int runNumber,string delimiter)` 

#### `public inline void `[`PushLine`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a450f8ca3d659f68fdb2d7b1ac07ec630)`(params IConvertible[] strings)` 

Push a line to the logger. a file

#### Parameters
* `line`

#### `public inline void `[`PushString`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a7de8cce7b2ce75b9aaf49bf10ed2f6ac)`(string str)` 

Push a string to the logger. Will throw an exception if the logger hasn't been opened

#### Parameters
* `str`

#### `public inline void `[`CloseFile`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ad80e25518f6a0ba4af943695cfed4d95)`()` 

#### `public inline void `[`CloseFile`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a10c1499e6315338e7e1194a50baf001e)`(string userName,string bucket,string serverPath)` 

#### `protected const string `[`dateTimeFormat`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ac11f47be888b85c7ba1bd27c6c354b85) 

#### `protected const string `[`bgcExtension`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ade142db0b9a30030077b3d1b5006da33) 

#### `protected string `[`delimiter`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a3004e4c89ccf17420c790e9401a3bb6e) 

#### `protected abstract `[`JsonObject`](#class_light_json_1_1_json_object)` `[`ConstructColumnMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1af43a9c04992c29f14686307c803c00fe)`()` 

#### `protected abstract `[`JsonObject`](#class_light_json_1_1_json_object)` `[`ConstructValueMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ad4af09334bc0c1ca6d2af991a33083bd)`()` 

#### `protected inline void `[`ApplyHeaders`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1ac3c823ee08460e7df8a839a8e6eddda9)`()` 

#### `protected inline string `[`GetNewLogName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1aa343bfd83dbffc1daa979f6c7a6a3be8)`(string userName,int runNumber,int session)` 

#### `protected inline string `[`GetSummaryFileName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a725994630d9161f4d356646e23823a91)`(string userName,int sessionNumber)` 

#### `protected inline string `[`GetExceptionFileName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a100a55da9153f0db61ef2f70d5ce100f)`()` 

#### `protected inline string `[`PathForLogFile`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a8e3d2b4713248800d39bda7b2a782c7e)`(string userName,string filename)` 

#### `protected inline void `[`ApplyRequiredFields`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logger_1a002e8058d833237a66cb935823b7af73)`(`[`JsonObject`](#class_light_json_1_1_json_object)` jo)` 

# class `BGC::IO::Logging::LoggingKeys` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public const string `[`GameName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a69374d0e2d29ccf48ba81bc28ee81283) | 
`public const string `[`Version`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a6ef397c5e00c48aa31a397609d1df1ab) | 
`public const string `[`UserName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1ac62f951e5a416c4eebd73c16d40f120e) | 
`public const string `[`Session`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1ae4c17b5ec70fe61b1bb950d4023e3c69) | 
`public const string `[`DeviceID`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1ab1221d95cb58100efdeaf17babafa64f) | 
`public const string `[`RunNumber`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a2a7932411f57faa251d1c6cbc223cdce) | 
`public const string `[`Delimiter`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1af1d02211120bc7c236f077eba6959741) | 
`public const string `[`ColumnMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1aa57688bd92f4da5f46c1335ee06f4dfe) | 
`public const string `[`DefaultColumn`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a920d7439a33781b7a9df116daab05710) | 
`public const string `[`ValueMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1aeb0f7b104b5725799dbb5514ac321227) | 

## Members

#### `public const string `[`GameName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a69374d0e2d29ccf48ba81bc28ee81283) 

#### `public const string `[`Version`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a6ef397c5e00c48aa31a397609d1df1ab) 

#### `public const string `[`UserName`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1ac62f951e5a416c4eebd73c16d40f120e) 

#### `public const string `[`Session`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1ae4c17b5ec70fe61b1bb950d4023e3c69) 

#### `public const string `[`DeviceID`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1ab1221d95cb58100efdeaf17babafa64f) 

#### `public const string `[`RunNumber`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a2a7932411f57faa251d1c6cbc223cdce) 

#### `public const string `[`Delimiter`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1af1d02211120bc7c236f077eba6959741) 

#### `public const string `[`ColumnMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1aa57688bd92f4da5f46c1335ee06f4dfe) 

#### `public const string `[`DefaultColumn`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1a920d7439a33781b7a9df116daab05710) 

#### `public const string `[`ValueMapping`](#class_b_g_c_1_1_i_o_1_1_logging_1_1_logging_keys_1aeb0f7b104b5725799dbb5514ac321227) 

# namespace `BGC::MonoUtility` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::MonoUtility::DestroyOnDestroy`](#class_b_g_c_1_1_mono_utility_1_1_destroy_on_destroy) | 

# class `BGC::MonoUtility::DestroyOnDestroy` 

```
class BGC::MonoUtility::DestroyOnDestroy
  : public MonoBehaviour
```  

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# namespace `BGC::UI` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::UI::ColorExtensions`](#class_b_g_c_1_1_u_i_1_1_color_extensions) | 
`class `[`BGC::UI::GraphicExtensions`](#class_b_g_c_1_1_u_i_1_1_graphic_extensions) | Set of extensions to image that allow for easy modification of the image's color.
`class `[`BGC::UI::ImageExtensions`](#class_b_g_c_1_1_u_i_1_1_image_extensions) | Set of extensions to image that allow for easy modification of the image's color.

# class `BGC::UI::ColorExtensions` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::UI::GraphicExtensions` 

Set of extensions to image that allow for easy modification of the image's color.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::UI::ImageExtensions` 

Set of extensions to image that allow for easy modification of the image's color.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# namespace `BGC::Utility` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::Utility::CoroutineUtility`](#class_b_g_c_1_1_utility_1_1_coroutine_utility) | 
`class `[`BGC::Utility::EmptyMonobehaviour`](#class_b_g_c_1_1_utility_1_1_empty_monobehaviour) | 
`class `[`BGC::Utility::EnumUtility`](#class_b_g_c_1_1_utility_1_1_enum_utility) | 
`class `[`BGC::Utility::IdManager`](#class_b_g_c_1_1_utility_1_1_id_manager) | 
`class `[`BGC::Utility::LogFilesTos3`](#class_b_g_c_1_1_utility_1_1_log_files_tos3) | 
`class `[`BGC::Utility::ReservedFiles`](#class_b_g_c_1_1_utility_1_1_reserved_files) | 

# class `BGC::Utility::CoroutineUtility` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::EmptyMonobehaviour` 

```
class BGC::Utility::EmptyMonobehaviour
  : public MonoBehaviour
```  

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::EnumUtility` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::IdManager` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::LogFilesTos3` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::ReservedFiles` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# namespace `BGC::Utility::Inspector` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::Utility::Inspector::ReadOnlyAttribute`](#class_b_g_c_1_1_utility_1_1_inspector_1_1_read_only_attribute) | 

# class `BGC::Utility::Inspector::ReadOnlyAttribute` 

```
class BGC::Utility::Inspector::ReadOnlyAttribute
  : public PropertyAttribute
```  

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# namespace `BGC::Utility::Math` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::Utility::Math::ByteMath`](#class_b_g_c_1_1_utility_1_1_math_1_1_byte_math) | 
`class `[`BGC::Utility::Math::Combinatorics`](#class_b_g_c_1_1_utility_1_1_math_1_1_combinatorics) | 
`class `[`BGC::Utility::Math::Conversion`](#class_b_g_c_1_1_utility_1_1_math_1_1_conversion) | 
`class `[`BGC::Utility::Math::CustomRandom`](#class_b_g_c_1_1_utility_1_1_math_1_1_custom_random) | 
`class `[`BGC::Utility::Math::GeneralMath`](#class_b_g_c_1_1_utility_1_1_math_1_1_general_math) | 
`class `[`BGC::Utility::Math::Probability`](#class_b_g_c_1_1_utility_1_1_math_1_1_probability) | 
`class `[`BGC::Utility::Math::SetOperations`](#class_b_g_c_1_1_utility_1_1_math_1_1_set_operations) | 

# class `BGC::Utility::Math::ByteMath` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::Math::Combinatorics` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::Math::Conversion` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::Math::CustomRandom` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::Math::GeneralMath` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::Math::Probability` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Utility::Math::SetOperations` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# namespace `BGC::Web` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::Web::AWSServer`](#class_b_g_c_1_1_web_1_1_a_w_s_server) | 

# class `BGC::Web::AWSServer` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public const string `[`ApiUrl`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a6b77ef9487de24fa44aa22c93e2c10a4) | 
`public const string `[`ApiKey`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a17297c455c5c500ad64eb7db00bfe1d0) | 
`public const string `[`CacheControl`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a1ddaac83c024fcd3cb7ec74f8b3b7cf0) | 
`public const string `[`BGCExtension`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a9643e723b971ab6a8a29f93b1cca75d6) | 

## Members

#### `public const string `[`ApiUrl`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a6b77ef9487de24fa44aa22c93e2c10a4) 

#### `public const string `[`ApiKey`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a17297c455c5c500ad64eb7db00bfe1d0) 

#### `public const string `[`CacheControl`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a1ddaac83c024fcd3cb7ec74f8b3b7cf0) 

#### `public const string `[`BGCExtension`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1a9643e723b971ab6a8a29f93b1cca75d6) 

# namespace `BGC::Web::Utility` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`BGC::Web::Utility::Rest`](#class_b_g_c_1_1_web_1_1_utility_1_1_rest) | 

# class `BGC::Web::Utility::Rest` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# namespace `LightJson` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`enum `[`JsonValueType`](#namespace_light_json_1a4c881ff1639e6c95644e020503380072)            | Enumerates the types of Json values.
`class `[`LightJson::JsonArray`](#class_light_json_1_1_json_array) | Represents an ordered collection of JsonValues.
`class `[`LightJson::JsonObject`](#class_light_json_1_1_json_object) | Represents a key-value pair collection of [JsonValue](#struct_light_json_1_1_json_value) objects.
`struct `[`LightJson::JsonValue`](#struct_light_json_1_1_json_value) | A wrapper object that contains a valid JSON value.

## Members

#### `enum `[`JsonValueType`](#namespace_light_json_1a4c881ff1639e6c95644e020503380072) 

 Values                         | Descriptions                                
--------------------------------|---------------------------------------------
Null            | A null value.
Boolean            | A boolean value.
Number            | A number value.
String            | A string value.
Object            | An object value.
Array            | An array value.

Enumerates the types of Json values.

# class `LightJson::JsonArray` 

```
class LightJson::JsonArray
  : public IEnumerable< JsonValue >
```  

Represents an ordered collection of JsonValues.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonArray`](#class_light_json_1_1_json_array_1ae32cc8acdd9b2c41d329d686e2b19c8b)`()` | Initializes a new instance of [JsonArray](#class_light_json_1_1_json_array).
`public inline  `[`JsonArray`](#class_light_json_1_1_json_array_1ae0f33f8a6a3935079ea82a0279db034d)`(params `[`JsonValue`](#struct_light_json_1_1_json_value)` values)` | Initializes a new instance of [JsonArray](#class_light_json_1_1_json_array), adding the given values to the collection.
`public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Add`](#class_light_json_1_1_json_array_1a4fec00492075faec359adfd0516079f6)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` | Adds the given value to this collection.
`public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Insert`](#class_light_json_1_1_json_array_1a827b534730281f6ca23030f8905816ed)`(int index,`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` | Inserts the given value at the given index in this collection.
`public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Remove`](#class_light_json_1_1_json_array_1a8d7975b6ec154d649b811e712821df30)`(int index)` | Removes the value at the given index.
`public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Clear`](#class_light_json_1_1_json_array_1a06619d5afcf780f16fef53a111990873)`()` | Clears the contents of this collection.
`public inline bool `[`Contains`](#class_light_json_1_1_json_array_1aa5afbb6901cf3a3c510308bbd4ebff53)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` item)` | Determines whether the given item is in the [JsonArray](#class_light_json_1_1_json_array).
`public inline int `[`IndexOf`](#class_light_json_1_1_json_array_1ae249097cc6b9fbbf75820a38fe64f013)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` item)` | Determines the index of the given item in this [JsonArray](#class_light_json_1_1_json_array).
`public inline `[`IEnumerator](#class_i_enumerator)< [JsonValue`](#struct_light_json_1_1_json_value)` > `[`GetEnumerator`](#class_light_json_1_1_json_array_1ae19f189aac5c20140f0e062950a3d452)`()` | Returns an enumerator that iterates through the collection.
`public inline override string `[`ToString`](#class_light_json_1_1_json_array_1a9a9f767d21d0bb473cdc85a633fbfa24)`()` | Returns a JSON string representing the state of the array.
`public inline string `[`ToString`](#class_light_json_1_1_json_array_1a52b674abdd7bb49d7614d0c81405e650)`(bool pretty)` | Returns a JSON string representing the state of the array.

## Members

#### `public inline  `[`JsonArray`](#class_light_json_1_1_json_array_1ae32cc8acdd9b2c41d329d686e2b19c8b)`()` 

Initializes a new instance of [JsonArray](#class_light_json_1_1_json_array).

#### `public inline  `[`JsonArray`](#class_light_json_1_1_json_array_1ae0f33f8a6a3935079ea82a0279db034d)`(params `[`JsonValue`](#struct_light_json_1_1_json_value)` values)` 

Initializes a new instance of [JsonArray](#class_light_json_1_1_json_array), adding the given values to the collection.

#### Parameters
* `values` The values to be added to this collection.

#### `public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Add`](#class_light_json_1_1_json_array_1a4fec00492075faec359adfd0516079f6)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` 

Adds the given value to this collection.

#### Parameters
* `value` The value to be added.

#### Returns
Returns this collection.

#### `public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Insert`](#class_light_json_1_1_json_array_1a827b534730281f6ca23030f8905816ed)`(int index,`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` 

Inserts the given value at the given index in this collection.

#### Parameters
* `index` The index where the given value will be inserted.

* `value` The value to be inserted into this collection.

#### Returns
Returns this collection.

#### `public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Remove`](#class_light_json_1_1_json_array_1a8d7975b6ec154d649b811e712821df30)`(int index)` 

Removes the value at the given index.

#### Parameters
* `index` The index of the value to be removed.

#### Returns
Return this collection.

#### `public inline `[`JsonArray`](#class_light_json_1_1_json_array)` `[`Clear`](#class_light_json_1_1_json_array_1a06619d5afcf780f16fef53a111990873)`()` 

Clears the contents of this collection.

#### Returns
Returns this collection.

#### `public inline bool `[`Contains`](#class_light_json_1_1_json_array_1aa5afbb6901cf3a3c510308bbd4ebff53)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` item)` 

Determines whether the given item is in the [JsonArray](#class_light_json_1_1_json_array).

#### Parameters
* `item` The item to locate in the [JsonArray](#class_light_json_1_1_json_array).

#### Returns
Returns true if the item is found; otherwise, false.

#### `public inline int `[`IndexOf`](#class_light_json_1_1_json_array_1ae249097cc6b9fbbf75820a38fe64f013)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` item)` 

Determines the index of the given item in this [JsonArray](#class_light_json_1_1_json_array).

#### Parameters
* `item` The item to locate in this [JsonArray](#class_light_json_1_1_json_array).

#### Returns
The index of the item, if found. Otherwise, returns -1.

#### `public inline `[`IEnumerator](#class_i_enumerator)< [JsonValue`](#struct_light_json_1_1_json_value)` > `[`GetEnumerator`](#class_light_json_1_1_json_array_1ae19f189aac5c20140f0e062950a3d452)`()` 

Returns an enumerator that iterates through the collection.

#### `public inline override string `[`ToString`](#class_light_json_1_1_json_array_1a9a9f767d21d0bb473cdc85a633fbfa24)`()` 

Returns a JSON string representing the state of the array.

The resulting string is safe to be inserted as is into dynamically generated JavaScript or JSON code.

#### `public inline string `[`ToString`](#class_light_json_1_1_json_array_1a52b674abdd7bb49d7614d0c81405e650)`(bool pretty)` 

Returns a JSON string representing the state of the array.

The resulting string is safe to be inserted as is into dynamically generated JavaScript or JSON code. 

#### Parameters
* `pretty` Indicates whether the resulting string should be formatted for human-readability.

# class `LightJson::JsonObject` 

```
class LightJson::JsonObject
  : public IEnumerable< KeyValuePair< string, JsonValue >>
  : public IEnumerable< JsonValue >
```  

Represents a key-value pair collection of [JsonValue](#struct_light_json_1_1_json_value) objects.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonObject`](#class_light_json_1_1_json_object_1a0f4040f98e3c158f4d58169ac20190b8)`()` | Initializes a new instance of [JsonObject](#class_light_json_1_1_json_object).
`public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Add`](#class_light_json_1_1_json_object_1a6e1c73338d5bc119f44b49b15bbedd87)`(string key)` | Adds a key with a null value to this collection.
`public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Add`](#class_light_json_1_1_json_object_1a2cc644324f2d54d1a6202e7ac76da2d9)`(string key,`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` | Adds a value associated with a key to this collection.
`public inline bool `[`Remove`](#class_light_json_1_1_json_object_1adc98c25e04ed78ace30c5eaf20f04100)`(string key)` | Removes a property with the given key.
`public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Clear`](#class_light_json_1_1_json_object_1aa027f615ef98dca30da9cad244d6e5c9)`()` | Clears the contents of this collection.
`public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Rename`](#class_light_json_1_1_json_object_1a8edbbc7db8b62b3790e48b88ed1fc199)`(string oldKey,string newKey)` | Changes the key of one of the items in the collection.
`public inline bool `[`ContainsKey`](#class_light_json_1_1_json_object_1a5d89b8efa4d1c8671cdeccb0008597bd)`(string key)` | Determines whether this collection contains an item assosiated with the given key.
`public inline bool `[`Contains`](#class_light_json_1_1_json_object_1a27dcb7437604de0852b649766d43632b)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` | Determines whether this collection contains the given [JsonValue](#struct_light_json_1_1_json_value).
`public inline `[`IEnumerator](#class_i_enumerator)< KeyValuePair< string, [JsonValue`](#struct_light_json_1_1_json_value)` > > `[`GetEnumerator`](#class_light_json_1_1_json_object_1a4ca7bec1fea376aeedfec84d26a0dec3)`()` | Returns an enumerator that iterates through this collection.
`public inline override string `[`ToString`](#class_light_json_1_1_json_object_1a6fb02c496d1126ea498b0061c0ec2ace)`()` | Returns a JSON string representing the state of the object.
`public inline string `[`ToString`](#class_light_json_1_1_json_object_1adcccb374aa5613d37e71c86766b7ed74)`(bool pretty)` | Returns a JSON string representing the state of the object.

## Members

#### `public inline  `[`JsonObject`](#class_light_json_1_1_json_object_1a0f4040f98e3c158f4d58169ac20190b8)`()` 

Initializes a new instance of [JsonObject](#class_light_json_1_1_json_object).

#### `public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Add`](#class_light_json_1_1_json_object_1a6e1c73338d5bc119f44b49b15bbedd87)`(string key)` 

Adds a key with a null value to this collection.

#### Parameters
* `key` The key of the property to be added.

Returns this [JsonObject](#class_light_json_1_1_json_object).

#### `public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Add`](#class_light_json_1_1_json_object_1a2cc644324f2d54d1a6202e7ac76da2d9)`(string key,`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` 

Adds a value associated with a key to this collection.

#### Parameters
* `key` The key of the property to be added.

* `value` The value of the property to be added.

#### Returns
Returns this [JsonObject](#class_light_json_1_1_json_object).

#### `public inline bool `[`Remove`](#class_light_json_1_1_json_object_1adc98c25e04ed78ace30c5eaf20f04100)`(string key)` 

Removes a property with the given key.

#### Parameters
* `key` The key of the property to be removed.

#### Returns
Returns true if the given key is found and removed; otherwise, false.

#### `public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Clear`](#class_light_json_1_1_json_object_1aa027f615ef98dca30da9cad244d6e5c9)`()` 

Clears the contents of this collection.

#### Returns
Returns this [JsonObject](#class_light_json_1_1_json_object).

#### `public inline `[`JsonObject`](#class_light_json_1_1_json_object)` `[`Rename`](#class_light_json_1_1_json_object_1a8edbbc7db8b62b3790e48b88ed1fc199)`(string oldKey,string newKey)` 

Changes the key of one of the items in the collection.

This method has no effects if the *oldKey* does not exists. If the *newKey* already exists, the value will be overwritten. 

#### Parameters
* `oldKey` The name of the key to be changed.

* `newKey` The new name of the key.

#### Returns
Returns this [JsonObject](#class_light_json_1_1_json_object).

#### `public inline bool `[`ContainsKey`](#class_light_json_1_1_json_object_1a5d89b8efa4d1c8671cdeccb0008597bd)`(string key)` 

Determines whether this collection contains an item assosiated with the given key.

#### Parameters
* `key` The key to locate in this collection.

#### Returns
Returns true if the key is found; otherwise, false.

#### `public inline bool `[`Contains`](#class_light_json_1_1_json_object_1a27dcb7437604de0852b649766d43632b)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` 

Determines whether this collection contains the given [JsonValue](#struct_light_json_1_1_json_value).

#### Parameters
* `value` The value to locate in this collection.

#### Returns
Returns true if the value is found; otherwise, false.

#### `public inline `[`IEnumerator](#class_i_enumerator)< KeyValuePair< string, [JsonValue`](#struct_light_json_1_1_json_value)` > > `[`GetEnumerator`](#class_light_json_1_1_json_object_1a4ca7bec1fea376aeedfec84d26a0dec3)`()` 

Returns an enumerator that iterates through this collection.

#### `public inline override string `[`ToString`](#class_light_json_1_1_json_object_1a6fb02c496d1126ea498b0061c0ec2ace)`()` 

Returns a JSON string representing the state of the object.

The resulting string is safe to be inserted as is into dynamically generated JavaScript or JSON code.

#### `public inline string `[`ToString`](#class_light_json_1_1_json_object_1adcccb374aa5613d37e71c86766b7ed74)`(bool pretty)` 

Returns a JSON string representing the state of the object.

The resulting string is safe to be inserted as is into dynamically generated JavaScript or JSON code. 

#### Parameters
* `pretty` Indicates whether the resulting string should be formatted for human-readability.

# struct `LightJson::JsonValue` 

A wrapper object that contains a valid JSON value.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1afe1590a96226b96ffd244bda3abe9a36)`(bool? value)` | Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a Boolean value.
`public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1ac924b9376a443c78ac623942da0c250f)`(double? value)` | Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a Number value.
`public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1a416380937e88df42791a238c1d5aae96)`(string value)` | Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a String value.
`public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1a35f0a45bca47f6152f05edabf214c9ae)`(`[`JsonObject`](#class_light_json_1_1_json_object)` value)` | Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a [JsonObject](#class_light_json_1_1_json_object).
`public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1a13d54d60d670ab8b78334e71bb5e8bca)`(`[`JsonArray`](#class_light_json_1_1_json_array)` value)` | Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a Array reference value.
`public inline override bool `[`Equals`](#struct_light_json_1_1_json_value_1abe1592e2bafd40680852ad64b995af5d)`(object obj)` | Returns a value indicating whether this [JsonValue](#struct_light_json_1_1_json_value) is equal to the given object.
`public inline override int `[`GetHashCode`](#struct_light_json_1_1_json_value_1ae0e1b9ad76473a4c7a3524661c011987)`()` | Returns a hash code for this [JsonValue](#struct_light_json_1_1_json_value).
`public inline override string `[`ToString`](#struct_light_json_1_1_json_value_1a168567e4dc7688f0bffab2e5382f47b5)`()` | Returns a JSON string representing the state of the object.
`public inline string `[`ToString`](#struct_light_json_1_1_json_value_1a2c3d9d9e761c81a87e7c5618f7cd28e4)`(bool pretty)` | Returns a JSON string representing the state of the object.

## Members

#### `public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1afe1590a96226b96ffd244bda3abe9a36)`(bool? value)` 

Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a Boolean value.

#### Parameters
* `value` The value to be wrapped.

#### `public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1ac924b9376a443c78ac623942da0c250f)`(double? value)` 

Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a Number value.

#### Parameters
* `value` The value to be wrapped.

#### `public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1a416380937e88df42791a238c1d5aae96)`(string value)` 

Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a String value.

#### Parameters
* `value` The value to be wrapped.

#### `public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1a35f0a45bca47f6152f05edabf214c9ae)`(`[`JsonObject`](#class_light_json_1_1_json_object)` value)` 

Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a [JsonObject](#class_light_json_1_1_json_object).

#### Parameters
* `value` The value to be wrapped.

#### `public inline  `[`JsonValue`](#struct_light_json_1_1_json_value_1a13d54d60d670ab8b78334e71bb5e8bca)`(`[`JsonArray`](#class_light_json_1_1_json_array)` value)` 

Initializes a new instance of the [JsonValue](#struct_light_json_1_1_json_value) struct, representing a Array reference value.

#### Parameters
* `value` The value to be wrapped.

#### `public inline override bool `[`Equals`](#struct_light_json_1_1_json_value_1abe1592e2bafd40680852ad64b995af5d)`(object obj)` 

Returns a value indicating whether this [JsonValue](#struct_light_json_1_1_json_value) is equal to the given object.

#### Parameters
* `obj` The object to test.

#### `public inline override int `[`GetHashCode`](#struct_light_json_1_1_json_value_1ae0e1b9ad76473a4c7a3524661c011987)`()` 

Returns a hash code for this [JsonValue](#struct_light_json_1_1_json_value).

#### `public inline override string `[`ToString`](#struct_light_json_1_1_json_value_1a168567e4dc7688f0bffab2e5382f47b5)`()` 

Returns a JSON string representing the state of the object.

The resulting string is safe to be inserted as is into dynamically generated JavaScript or JSON code.

#### `public inline string `[`ToString`](#struct_light_json_1_1_json_value_1a2c3d9d9e761c81a87e7c5618f7cd28e4)`(bool pretty)` 

Returns a JSON string representing the state of the object.

The resulting string is safe to be inserted as is into dynamically generated JavaScript or JSON code. 

#### Parameters
* `pretty` Indicates whether the resulting string should be formatted for human-readability.

# namespace `LightJson::Serialization` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`class `[`LightJson::Serialization::JsonParseException`](#class_light_json_1_1_serialization_1_1_json_parse_exception) | The exception that is thrown when a JSON message cannot be parsed.
`class `[`LightJson::Serialization::JsonReader`](#class_light_json_1_1_serialization_1_1_json_reader) | Represents a reader that can read JsonValues.
`class `[`LightJson::Serialization::JsonSerializationException`](#class_light_json_1_1_serialization_1_1_json_serialization_exception) | The exception that is thrown when a JSON value cannot be serialized.
`class `[`LightJson::Serialization::JsonWriter`](#class_light_json_1_1_serialization_1_1_json_writer) | Represents a writer that can write string representations of JsonValues.
`class `[`LightJson::Serialization::TextScanner`](#class_light_json_1_1_serialization_1_1_text_scanner) | Represents a text scanner that reads one character at a time.
`struct `[`LightJson::Serialization::TextPosition`](#struct_light_json_1_1_serialization_1_1_text_position) | Represents a position within a plain text resource.

# class `LightJson::Serialization::JsonParseException` 

```
class LightJson::Serialization::JsonParseException
  : public Exception
```  

The exception that is thrown when a JSON message cannot be parsed.

This exception is only intended to be thrown by [LightJson](#namespace_light_json).

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonParseException`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1acb5df85d11392122cfb6992b22bb2b21)`()` | Initializes a new instance of [JsonParseException](#class_light_json_1_1_serialization_1_1_json_parse_exception).
`public inline  `[`JsonParseException`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a5fa3df142295ebde043b2566f6093a3d)`(`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a9157596805755c8307bfbd1b3e937684)` type,`[`TextPosition`](#struct_light_json_1_1_serialization_1_1_text_position)` position)` | Initializes a new instance of [JsonParseException](#class_light_json_1_1_serialization_1_1_json_parse_exception) with the given error type and position.
`public inline  `[`JsonParseException`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a7ebac47381d0a5fe717e28010c3a0875)`(string message,`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a9157596805755c8307bfbd1b3e937684)` type,`[`TextPosition`](#struct_light_json_1_1_serialization_1_1_text_position)` position)` | Initializes a new instance of [JsonParseException](#class_light_json_1_1_serialization_1_1_json_parse_exception) with the given message, error type, and position.

## Members

#### `public inline  `[`JsonParseException`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1acb5df85d11392122cfb6992b22bb2b21)`()` 

Initializes a new instance of [JsonParseException](#class_light_json_1_1_serialization_1_1_json_parse_exception).

#### `public inline  `[`JsonParseException`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a5fa3df142295ebde043b2566f6093a3d)`(`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a9157596805755c8307bfbd1b3e937684)` type,`[`TextPosition`](#struct_light_json_1_1_serialization_1_1_text_position)` position)` 

Initializes a new instance of [JsonParseException](#class_light_json_1_1_serialization_1_1_json_parse_exception) with the given error type and position.

#### Parameters
* `type` The error type that describes the cause of the error.

* `position` The position in the text where the error occurred.

#### `public inline  `[`JsonParseException`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a7ebac47381d0a5fe717e28010c3a0875)`(string message,`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_parse_exception_1a9157596805755c8307bfbd1b3e937684)` type,`[`TextPosition`](#struct_light_json_1_1_serialization_1_1_text_position)` position)` 

Initializes a new instance of [JsonParseException](#class_light_json_1_1_serialization_1_1_json_parse_exception) with the given message, error type, and position.

#### Parameters
* `message` The message that describes the error.

* `type` The error type that describes the cause of the error.

* `position` The position in the text where the error occurred.

# class `LightJson::Serialization::JsonReader` 

Represents a reader that can read JsonValues.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `LightJson::Serialization::JsonSerializationException` 

```
class LightJson::Serialization::JsonSerializationException
  : public Exception
```  

The exception that is thrown when a JSON value cannot be serialized.

This exception is only intended to be thrown by [LightJson](#namespace_light_json).

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonSerializationException`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a1246095f09c9e2a13fe5f9900bb0b8e4)`()` | Initializes a new instance of [JsonSerializationException](#class_light_json_1_1_serialization_1_1_json_serialization_exception).
`public inline  `[`JsonSerializationException`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a4fba8349d350728ae1417e05edf6704b)`(`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a5f46993afb6bbbb9f3a3d8981281af20)` type)` | Initializes a new instance of [JsonSerializationException](#class_light_json_1_1_serialization_1_1_json_serialization_exception) with the given error type.
`public inline  `[`JsonSerializationException`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1af97ceaee9d7193df2a70b0ac5db33e63)`(string message,`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a5f46993afb6bbbb9f3a3d8981281af20)` type)` | Initializes a new instance of [JsonSerializationException](#class_light_json_1_1_serialization_1_1_json_serialization_exception) with the given message and error type.

## Members

#### `public inline  `[`JsonSerializationException`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a1246095f09c9e2a13fe5f9900bb0b8e4)`()` 

Initializes a new instance of [JsonSerializationException](#class_light_json_1_1_serialization_1_1_json_serialization_exception).

#### `public inline  `[`JsonSerializationException`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a4fba8349d350728ae1417e05edf6704b)`(`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a5f46993afb6bbbb9f3a3d8981281af20)` type)` 

Initializes a new instance of [JsonSerializationException](#class_light_json_1_1_serialization_1_1_json_serialization_exception) with the given error type.

#### Parameters
* `type` The error type that describes the cause of the error.

#### `public inline  `[`JsonSerializationException`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1af97ceaee9d7193df2a70b0ac5db33e63)`(string message,`[`ErrorType`](#class_light_json_1_1_serialization_1_1_json_serialization_exception_1a5f46993afb6bbbb9f3a3d8981281af20)` type)` 

Initializes a new instance of [JsonSerializationException](#class_light_json_1_1_serialization_1_1_json_serialization_exception) with the given message and error type.

#### Parameters
* `message` The message that describes the error.

* `type` The error type that describes the cause of the error.

# class `LightJson::Serialization::JsonWriter` 

```
class LightJson::Serialization::JsonWriter
  : public IDisposable
```  

Represents a writer that can write string representations of JsonValues.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonWriter`](#class_light_json_1_1_serialization_1_1_json_writer_1a941ea8e7c31ff7da8808746f81d7aff0)`()` | Initializes a new instance of [JsonWriter](#class_light_json_1_1_serialization_1_1_json_writer).
`public inline  `[`JsonWriter`](#class_light_json_1_1_serialization_1_1_json_writer_1a4a6b01db0ae19bfab52a15fff0bd1970)`(bool pretty)` | Initializes a new instance of [JsonWriter](#class_light_json_1_1_serialization_1_1_json_writer).
`public inline string `[`Serialize`](#class_light_json_1_1_serialization_1_1_json_writer_1a3d80710745c91fdbe678f976cdc908c6)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` jsonValue)` | Returns a string representation of the given [JsonValue](#struct_light_json_1_1_json_value).
`public inline void `[`Dispose`](#class_light_json_1_1_serialization_1_1_json_writer_1a1f69da8e4e0ef5831444d96a9f72f844)`()` | Releases all the resources used by this object.

## Members

#### `public inline  `[`JsonWriter`](#class_light_json_1_1_serialization_1_1_json_writer_1a941ea8e7c31ff7da8808746f81d7aff0)`()` 

Initializes a new instance of [JsonWriter](#class_light_json_1_1_serialization_1_1_json_writer).

#### `public inline  `[`JsonWriter`](#class_light_json_1_1_serialization_1_1_json_writer_1a4a6b01db0ae19bfab52a15fff0bd1970)`(bool pretty)` 

Initializes a new instance of [JsonWriter](#class_light_json_1_1_serialization_1_1_json_writer).

#### Parameters
* `pretty` A value indicating whether the output of the writer should be human-readable.

#### `public inline string `[`Serialize`](#class_light_json_1_1_serialization_1_1_json_writer_1a3d80710745c91fdbe678f976cdc908c6)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` jsonValue)` 

Returns a string representation of the given [JsonValue](#struct_light_json_1_1_json_value).

#### Parameters
* `jsonValue` The [JsonValue](#struct_light_json_1_1_json_value) to serialize.

#### `public inline void `[`Dispose`](#class_light_json_1_1_serialization_1_1_json_writer_1a1f69da8e4e0ef5831444d96a9f72f844)`()` 

Releases all the resources used by this object.

# class `LightJson::Serialization::TextScanner` 

Represents a text scanner that reads one character at a time.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`TextScanner`](#class_light_json_1_1_serialization_1_1_text_scanner_1a9799580b075706faec14cb502755660c)`(TextReader reader)` | Initializes a new instance of [TextScanner](#class_light_json_1_1_serialization_1_1_text_scanner).
`public inline char `[`Peek`](#class_light_json_1_1_serialization_1_1_text_scanner_1ae3dcba6bac7152398498a57be9f327f9)`()` | Reads the next character in the stream without changing the current position.
`public inline char `[`Read`](#class_light_json_1_1_serialization_1_1_text_scanner_1af240c088f37c2b70d9f898964e4e155f)`()` | Reads the next character in the stream, advancing the text position.
`public inline void `[`SkipWhitespace`](#class_light_json_1_1_serialization_1_1_text_scanner_1a5cb96d47ba47baf23c2202e0eb5db4b3)`()` | Advances the scanner to next non-whitespace character.
`public inline void `[`Assert`](#class_light_json_1_1_serialization_1_1_text_scanner_1a188dbfe6bf38640fe99cf63c70d57bbb)`(char next)` | Verifies that the given character matches the next character in the stream. If the characters do not match, an exception will be thrown.
`public inline void `[`Assert`](#class_light_json_1_1_serialization_1_1_text_scanner_1a420bd647b3b845546a239af0707069a8)`(string next)` | Verifies that the given string matches the next characters in the stream. If the strings do not match, an exception will be thrown.

## Members

#### `public inline  `[`TextScanner`](#class_light_json_1_1_serialization_1_1_text_scanner_1a9799580b075706faec14cb502755660c)`(TextReader reader)` 

Initializes a new instance of [TextScanner](#class_light_json_1_1_serialization_1_1_text_scanner).

#### Parameters
* `reader` The TextReader to read the text.

#### `public inline char `[`Peek`](#class_light_json_1_1_serialization_1_1_text_scanner_1ae3dcba6bac7152398498a57be9f327f9)`()` 

Reads the next character in the stream without changing the current position.

#### `public inline char `[`Read`](#class_light_json_1_1_serialization_1_1_text_scanner_1af240c088f37c2b70d9f898964e4e155f)`()` 

Reads the next character in the stream, advancing the text position.

#### `public inline void `[`SkipWhitespace`](#class_light_json_1_1_serialization_1_1_text_scanner_1a5cb96d47ba47baf23c2202e0eb5db4b3)`()` 

Advances the scanner to next non-whitespace character.

#### `public inline void `[`Assert`](#class_light_json_1_1_serialization_1_1_text_scanner_1a188dbfe6bf38640fe99cf63c70d57bbb)`(char next)` 

Verifies that the given character matches the next character in the stream. If the characters do not match, an exception will be thrown.

#### Parameters
* `next` The expected character.

#### `public inline void `[`Assert`](#class_light_json_1_1_serialization_1_1_text_scanner_1a420bd647b3b845546a239af0707069a8)`(string next)` 

Verifies that the given string matches the next characters in the stream. If the strings do not match, an exception will be thrown.

#### Parameters
* `next` The expected string.

# struct `LightJson::Serialization::TextPosition` 

Represents a position within a plain text resource.

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public long `[`column`](#struct_light_json_1_1_serialization_1_1_text_position_1aac03bf3e1eaa383b32bfaa75622eb2f6) | The column position, 0-based.
`public long `[`line`](#struct_light_json_1_1_serialization_1_1_text_position_1a216a047b931194544c19b8ba57c81066) | The line position, 0-based.

## Members

#### `public long `[`column`](#struct_light_json_1_1_serialization_1_1_text_position_1aac03bf3e1eaa383b32bfaa75622eb2f6) 

The column position, 0-based.

#### `public long `[`line`](#struct_light_json_1_1_serialization_1_1_text_position_1a216a047b931194544c19b8ba57c81066) 

The line position, 0-based.

# class `BGC::Web::AWSServer::BodyKeys` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public const string `[`Bucket`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_body_keys_1a01fab75ad6d7680fded8ad06678b286a) | 
`public const string `[`Path`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_body_keys_1ab56d1c742479b553be9cf6681a62a37e) | 
`public const string `[`Content`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_body_keys_1a547accda3d0c1aa3a0053f6bac5ab294) | 

## Members

#### `public const string `[`Bucket`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_body_keys_1a01fab75ad6d7680fded8ad06678b286a) 

#### `public const string `[`Path`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_body_keys_1ab56d1c742479b553be9cf6681a62a37e) 

#### `public const string `[`Content`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_body_keys_1a547accda3d0c1aa3a0053f6bac5ab294) 

# class `Exception` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `BGC::Web::AWSServer::HeaderKeys` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public const string `[`ApiKey`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_header_keys_1ae45b31d3912b0c3274abe58e0c9217f4) | 
`public const string `[`CacheControl`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_header_keys_1a46899841cfd1dbdb20b94f5f9b5f8f26) | 

## Members

#### `public const string `[`ApiKey`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_header_keys_1ae45b31d3912b0c3274abe58e0c9217f4) 

#### `public const string `[`CacheControl`](#class_b_g_c_1_1_web_1_1_a_w_s_server_1_1_header_keys_1a46899841cfd1dbdb20b94f5f9b5f8f26) 

# class `ICollection` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `IDisposable` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `IEnumerable` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `IEnumerable< KeyValuePair< string, JsonValue >>` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `IEnumerator` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `LightJson::JsonArray::JsonArrayDebugView` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonArrayDebugView`](#class_light_json_1_1_json_array_1_1_json_array_debug_view_1ad890eb6fa60f116b0fdac7bf6a194422)`(`[`JsonArray`](#class_light_json_1_1_json_array)` jsonArray)` | 

## Members

#### `public inline  `[`JsonArrayDebugView`](#class_light_json_1_1_json_array_1_1_json_array_debug_view_1ad890eb6fa60f116b0fdac7bf6a194422)`(`[`JsonArray`](#class_light_json_1_1_json_array)` jsonArray)` 

# class `LightJson::JsonObject::JsonObjectDebugView` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonObjectDebugView`](#class_light_json_1_1_json_object_1_1_json_object_debug_view_1abc8f2f1fa0008d8dc295f5f9d26d0ab5)`(`[`JsonObject`](#class_light_json_1_1_json_object)` jsonObject)` | 

## Members

#### `public inline  `[`JsonObjectDebugView`](#class_light_json_1_1_json_object_1_1_json_object_debug_view_1abc8f2f1fa0008d8dc295f5f9d26d0ab5)`(`[`JsonObject`](#class_light_json_1_1_json_object)` jsonObject)` 

# class `LightJson::JsonValue::JsonValueDebugView` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`JsonValueDebugView`](#class_light_json_1_1_json_value_1_1_json_value_debug_view_1ab3ec2331cfbd4ecd74f7fe40cd4e8611)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` jsonValue)` | 

## Members

#### `public inline  `[`JsonValueDebugView`](#class_light_json_1_1_json_value_1_1_json_value_debug_view_1ab3ec2331cfbd4ecd74f7fe40cd4e8611)`(`[`JsonValue`](#struct_light_json_1_1_json_value)` jsonValue)` 

# class `LightJson::JsonObject::JsonObjectDebugView::KeyValuePair` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------
`public inline  `[`KeyValuePair`](#class_light_json_1_1_json_object_1_1_json_object_debug_view_1_1_key_value_pair_1a95ae927fe46e059f4a183f5d5b28669e)`(string key,`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` | 

## Members

#### `public inline  `[`KeyValuePair`](#class_light_json_1_1_json_object_1_1_json_object_debug_view_1_1_key_value_pair_1a95ae927fe46e059f4a183f5d5b28669e)`(string key,`[`JsonValue`](#struct_light_json_1_1_json_value)` value)` 

# class `MonoBehaviour` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

# class `PropertyAttribute` 

## Summary

 Members                        | Descriptions                                
--------------------------------|---------------------------------------------

## Members

Generated by [Moxygen](https://sourcey.com/moxygen)