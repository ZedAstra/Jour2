const API_HOSTNAME = "http://localhost:5000";

export async function uploadEntry(file: File): Promise<boolean | string[][]> {
    const form = new FormData();
    form.append("file", file);
    const result = await fetch(`${API_HOSTNAME}/api/upload`, {
        method: "POST",
        body: form
    });
    if(result.status === 202)
    {
        return await result.json();
    }
    else if(result.status === 400)
    {
        return false;
    }
    else
    {
        throw new Error(`Unexpected status code: ${result.status}`);
    }
}

export async function getEntries(): Promise<null | string[]> {
    const result = await fetch(`${API_HOSTNAME}/api/list`);
    if(result.status === 200)
    {
        return await result.json();
    }
    else return null;
}

export async function getEntry(entryName: string): Promise<null | string[][]> {
    const result = await fetch(`${API_HOSTNAME}/api/entry/${entryName}`);
    if(result.status === 200)
    {
        return await result.json();
    }
    else return null;
    
}