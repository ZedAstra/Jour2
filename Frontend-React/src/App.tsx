import { useEffect, useState } from 'react'
import * as api from './api'
import './App.css'

function App() {
  
  const [entries, setEntries] = useState<string[]>([]);
  const [selectedEntry, setSelectedEntry] = useState<string | null>(null);
  const [entryContent, setEntryContent] = useState<string[][]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    refresh();
  }, []);

  const [refreshing, setRefreshing] = useState(false);
  const [loadingEntry, setLoadingEntry] = useState(false);
  const [loadingWholePage, setLoadingWholePage] = useState(false);

  const [toastVisible, setToastVisible] = useState(false);
  const [toastMessage, setToastMessage] = useState("");
  const [toastType, setToastType] = useState<"success" | "neutral" | "error">("neutral");

  async function refresh() {
    setRefreshing(true);
    const data = await api.getEntries();
    if (data) {
      setSelectedEntry(null);
      setEntryContent([]);
      setEntries(data);
    }
    setRefreshing(false);
  }
  async function fetchEntry(name: string) {
    setLoadingEntry(true);
    const data = await api.getEntry(name);
    if (data) {
      setSelectedEntry(name);
      setEntryContent(data);
    } else {
      setSelectedEntry(null);
      setEntryContent([]);
    }
    setLoadingEntry(false);
  }
  async function submitFile() {
    const file = ((window as any).fileInput as HTMLInputElement).files?.[0];
    if (!file) {
      showToast("Please select a file to upload.", "error");
      return;
    }
    ((window as any).fileInput as HTMLInputElement).value = "";
    ((window as any).addNewDetails as HTMLDetailsElement).open = false;
    setLoadingWholePage(true);
    const data = await api.uploadEntry(file);
    setLoadingWholePage(false);
    if(!data) {
      showToast("Failed to upload file. Please try again.", "error");
      setLoadingWholePage(false);
    } else {
      showToast("File uploaded successfully!", "success");
      await refresh();
    }
  }

  function showToast(message: string | null, type: "success" | "neutral" | "error" = "neutral") {
    if (message) {
      setToastMessage(message);
      setToastType(type);
      setToastVisible(true);
      setTimeout(() => {
        setToastMessage("");
        setToastVisible(false);
        setError(null);
      }, 3000);
    }
  }

  return (
    <>
      <div className="w-screen">
        <div className="toast toast-end">
            {toastVisible && toastType === "success" && (
                <div className="alert alert-success">
                    <span>{toastMessage}</span>
                </div>
            )} 
            {toastVisible && toastType === "neutral" && (
                <div className="alert alert-info">
                    <span>{toastMessage}</span>
                </div>
            )} 
            {toastVisible && toastType === "error" && (
                <div className="alert alert-error">
                    <span>{toastMessage}</span>
                </div>
            )}
        </div>
        <div className="flex w-screen h-screen flex-row">
            <div className="flex flex-col p-2 w-96 h-full bg-neutral-200">
                <div className="join join-horizontal">
                    <button className={"btn btn-primary " + (refreshing ? " btn-disabled " : "") + "w-1/2 join-item"} onClick={refresh}>
                        Refresh
                        {refreshing && 
                          <span className="loading loading-spinner loading-sm"></span>}
                    </button>
                    <details id="addNewDetails" className="dropdown w-1/2">
                        <summary className="btn btn-success w-full join-item">Add Entry +</summary>
                        <ul className="menu dropdown-content bg-base-100 rounded-box z-1 w-80 p-2 shadow-sm">
                            <input id="fileInput" type="file" className="file-input"></input>
                            <button className="btn btn-secondary w-full" onClick={submitFile}>Submit</button>
                        </ul>
                    </details>
                </div>
                <div className="flex-1 overflow-y-scroll p-2 gap-2">
                  {entries.length == 0 && 
                    <div className="alert alert-warning">
                        <span>No entries found. Click refresh to load data.</span>
                    </div>
                  }
                  
                  {entries.map((entry, index) => (
                    <button key={index} className={"btn btn-info w-full " + (selectedEntry === entry ? "btn-disabled" : "")} onClick={() => fetchEntry(entry)}>
                        {entry}
                    </button>
                  ))}
                </div>
            </div>
            <div className="flex flex-1 p-2 bg-neutral-300 overflow-x-auto">
              {entryContent.length > 0 ? 
                (<table className="table w-full h-fit">
                  {entryContent.map((row, index) => (
                    index === 0 ?
                      (<thead key={index}>
                        <tr>
                            {row.map((header, index2) => (
                                <th key={index2} className="p-2 text-left">{header}</th>
                            ))}
                        </tr>
                      </thead>) 
                        : 
                      (<tbody key={index}>
                        <tr>
                          {row.map((cell, index2) => (
                              <td key={index2} className="p-2">
                                  {cell}
                              </td>
                          ))}
                        </tr>
                      </tbody>)
                  ))}
                </table>) : "?"}
            </div>
        </div>
        {loadingWholePage && (
            <div className="fixed inset-0 flex items-center justify-center bg-neutral-100 bg-opacity-50 z-50">
                <span className="loading loading-spinner loading-lg"></span>
            </div>
        )}
      </div>
    </>
  )
}

export default App
