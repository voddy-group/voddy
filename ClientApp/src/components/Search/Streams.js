import React, {useState, useEffect} from "react";
import RenderSearchRow from "./RenderRow";
import "../../assets/styles/StreamSearch.css";


export default function Streams() {
    const [searchValue, setSearchValue] = useState("");
    const [searchData, setSearchData] = useState( []);

    function handleChangeSearchValue(e) {
        setSearchValue(e.target.value);
    }
    
    useEffect(() => {
        const delayDebounceFn = setTimeout(() => getSearch(), 1500)
        return () => clearTimeout(delayDebounceFn)
    }, [searchValue])
    
    async function getSearch() {
        if (searchValue !== void(0) && searchValue !== "") {
            const response = await fetch('twitchApi/stream/search' +
                '?term=' + searchValue, {
                Method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            const returnedData = await response.json();
            
            setSearchData(returnedData.data);
        }
    }
    
    async function getStreams() {
        
    }

    return (
        <div>
            <h1>Search for streams</h1>
            <input onChange={handleChangeSearchValue} />
            <table>
                <tbody>
                    {  searchData.map(data => <RenderSearchRow data={data} />) }
                </tbody>
            </table>
        </div>
    )
}