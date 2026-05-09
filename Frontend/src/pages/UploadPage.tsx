import React, { useState } from 'react';
import { Upload, File, Folder, AlertCircle, CheckCircle } from 'lucide-react';
import { useDropzone } from 'react-dropzone';
import api from '../services/api';

interface UploadResponse {
  success: boolean;
  message: string;
  data: Array<{
    id: number;
    studentId: string;
    studentName: string;
    plagiarismScore: number;
  }> | {
    id: number;
    studentId: string;
    studentName: string;
    plagiarismScore: number;
  };
}

function UploadPage() {
  const [uploadMode, setUploadMode] = useState<'single' | 'bulk'>('single');
  const [loading, setLoading] = useState(false);
  const [uploadResult, setUploadResult] = useState<UploadResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  
  // Single file form
  const [singleForm, setSingleForm] = useState({
    studentId: '',
    studentName: '',
    assignmentName: '',
    file: null as File | null,
  });

  // Bulk upload form
  const [bulkForm, setBulkForm] = useState({
    assignmentName: '',
    semester: '',
    file: null as File | null,
  });

  const { getRootProps: getSingleRootProps, getInputProps: getSingleInputProps } = useDropzone({
    onDrop: (files) => {
      if (files.length > 0) {
        setSingleForm({ ...singleForm, file: files[0] });
      }
    },
    accept: {
      'application/xml': ['.fprg', '.frpg', '.fgl'],
      'text/xml': ['.fprg', '.frpg', '.fgl'],
    },
    multiple: false,
  });

  const { getRootProps: getBulkRootProps, getInputProps: getBulkInputProps } = useDropzone({
    onDrop: (files) => {
      if (files.length > 0) {
        setBulkForm({ ...bulkForm, file: files[0] });
      }
    },
    accept: { 'application/zip': ['.zip'] },
    multiple: false,
  });

  const handleSingleUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!singleForm.file || !singleForm.studentId || !singleForm.studentName || !singleForm.assignmentName) {
      setError('Please fill all fields');
      return;
    }

    setLoading(true);
    setError(null);

    const formData = new FormData();
    formData.append('file', singleForm.file);
    formData.append('studentId', singleForm.studentId);
    formData.append('studentName', singleForm.studentName);
    formData.append('assignmentName', singleForm.assignmentName);
    formData.append('submissionDate', new Date().toISOString());

    try {
      const response = await api.post('/api/analysis/upload-single', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setUploadResult(response.data);
      setSingleForm({ studentId: '', studentName: '', assignmentName: '', file: null });
    } catch (err: any) {
      setError(err.response?.data?.message || 'Upload failed');
    } finally {
      setLoading(false);
    }
  };

  const handleBulkUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!bulkForm.file || !bulkForm.assignmentName) {
      setError('Please fill all fields');
      return;
    }

    setLoading(true);
    setError(null);

    const formData = new FormData();
    formData.append('zipFile', bulkForm.file);
    formData.append('assignmentName', bulkForm.assignmentName);
    if (bulkForm.semester) {
      formData.append('semester', bulkForm.semester);
    }

    try {
      const response = await api.post('/api/analysis/upload-bulk', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      setUploadResult(response.data);
      setBulkForm({ assignmentName: '', semester: '', file: null });
    } catch (err: any) {
      setError(err.response?.data?.message || 'Upload failed');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-6xl mx-auto px-4 py-8">
      <div className="mb-8">
        <h2 className="text-4xl font-bold text-gray-900 mb-2">Upload Files</h2>
        <p className="text-gray-600">Upload Flowgorithm files for analysis</p>
      </div>

      {/* Mode Selection */}
      <div className="mb-8 flex gap-4">
        <button
          onClick={() => setUploadMode('single')}
          className={`px-6 py-3 rounded-lg font-semibold transition-all ${
            uploadMode === 'single'
              ? 'bg-gradient-to-r from-blue-600 to-cyan-600 text-white shadow-lg'
              : 'bg-white text-gray-700 border-2 border-gray-300 hover:border-blue-600'
          }`}
        >
          <File className="inline mr-2 w-5 h-5" />
          Single File
        </button>
        <button
          onClick={() => setUploadMode('bulk')}
          className={`px-6 py-3 rounded-lg font-semibold transition-all ${
            uploadMode === 'bulk'
              ? 'bg-gradient-to-r from-blue-600 to-cyan-600 text-white shadow-lg'
              : 'bg-white text-gray-700 border-2 border-gray-300 hover:border-blue-600'
          }`}
        >
          <Folder className="inline mr-2 w-5 h-5" />
          Bulk (ZIP)
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
        {/* Upload Form */}
        <div className="bg-white rounded-lg shadow-lg p-8 border-t-4 border-blue-600">
          {uploadMode === 'single' ? (
            <form onSubmit={handleSingleUpload}>
              <h3 className="text-2xl font-bold mb-6 text-gray-900">Single File Upload</h3>

              <div className="mb-4">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Student ID *
                </label>
                <input
                  type="text"
                  value={singleForm.studentId}
                  onChange={(e) => setSingleForm({ ...singleForm, studentId: e.target.value })}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-blue-600"
                  placeholder="e.g., STU001"
                />
              </div>

              <div className="mb-4">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Student Name *
                </label>
                <input
                  type="text"
                  value={singleForm.studentName}
                  onChange={(e) => setSingleForm({ ...singleForm, studentName: e.target.value })}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-blue-600"
                  placeholder="e.g., John Doe"
                />
              </div>

              <div className="mb-4">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Assignment Name *
                </label>
                <input
                  type="text"
                  value={singleForm.assignmentName}
                  onChange={(e) => setSingleForm({ ...singleForm, assignmentName: e.target.value })}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-blue-600"
                  placeholder="e.g., Assignment 1"
                />
              </div>

              <div className="mb-6">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Flowgorithm File (.fprg, .frpg, .fgl) *
                </label>
                <div
                  {...getSingleRootProps()}
                  className="border-2 border-dashed border-blue-600 rounded-lg p-8 text-center cursor-pointer hover:bg-blue-50 transition"
                >
                  <input {...getSingleInputProps()} />
                  <Upload className="w-12 h-12 text-blue-600 mx-auto mb-2" />
                  <p className="font-semibold text-gray-900">
                    {singleForm.file ? singleForm.file.name : 'Drag or click to select file'}
                  </p>
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-gradient-to-r from-blue-600 to-cyan-600 text-white py-3 rounded-lg font-bold hover:shadow-lg transition-all disabled:opacity-50"
              >
                {loading ? 'Uploading...' : 'Upload & Analyze'}
              </button>
            </form>
          ) : (
            <form onSubmit={handleBulkUpload}>
              <h3 className="text-2xl font-bold mb-6 text-gray-900">Bulk Upload (ZIP)</h3>

              <div className="mb-4">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Assignment Name *
                </label>
                <input
                  type="text"
                  value={bulkForm.assignmentName}
                  onChange={(e) => setBulkForm({ ...bulkForm, assignmentName: e.target.value })}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-blue-600"
                  placeholder="e.g., Assignment 1"
                />
              </div>

              <div className="mb-4">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  Semester (Optional)
                </label>
                <input
                  type="text"
                  value={bulkForm.semester}
                  onChange={(e) => setBulkForm({ ...bulkForm, semester: e.target.value })}
                  className="w-full px-4 py-2 border-2 border-gray-300 rounded-lg focus:outline-none focus:border-blue-600"
                  placeholder="e.g., Spring 2024"
                />
              </div>

              <div className="mb-6">
                <label className="block text-sm font-semibold text-gray-700 mb-2">
                  ZIP File *
                </label>
                <div
                  {...getBulkRootProps()}
                  className="border-2 border-dashed border-blue-600 rounded-lg p-8 text-center cursor-pointer hover:bg-blue-50 transition"
                >
                  <input {...getBulkInputProps()} />
                  <Folder className="w-12 h-12 text-blue-600 mx-auto mb-2" />
                  <p className="font-semibold text-gray-900">
                    {bulkForm.file ? bulkForm.file.name : 'Drag or click to select ZIP file'}
                  </p>
                  <p className="text-sm text-gray-600 mt-2">
                    Contains multiple .fprg, .frpg, or .fgl files with naming: StudentID_Name_file.fprg
                  </p>
                </div>
              </div>

              <button
                type="submit"
                disabled={loading}
                className="w-full bg-gradient-to-r from-blue-600 to-cyan-600 text-white py-3 rounded-lg font-bold hover:shadow-lg transition-all disabled:opacity-50"
              >
                {loading ? 'Uploading...' : 'Upload & Analyze All'}
              </button>
            </form>
          )}
        </div>

        {/* Results Section */}
        <div>
          {error && (
            <div className="bg-red-50 border-2 border-red-600 rounded-lg p-6 mb-6">
              <div className="flex items-center gap-3">
                <AlertCircle className="w-6 h-6 text-red-600" />
                <div>
                  <h4 className="font-bold text-red-900">Error</h4>
                  <p className="text-red-700">{error}</p>
                </div>
              </div>
            </div>
          )}

          {uploadResult && (
            <div className="bg-green-50 border-2 border-green-600 rounded-lg p-6">
              <div className="flex items-center gap-3 mb-4">
                <CheckCircle className="w-6 h-6 text-green-600" />
                <div>
                  <h4 className="font-bold text-green-900">Success!</h4>
                  <p className="text-green-700">{uploadResult.message}</p>
                </div>
              </div>

              <div className="bg-white rounded-lg p-4 mt-4">
                <h4 className="font-bold text-gray-900 mb-3">Results:</h4>
                {Array.isArray(uploadResult.data) ? (
                  <div className="space-y-2">
                    {uploadResult.data.map((item: any, idx) => (
                      <div key={idx} className="flex justify-between items-center p-2 bg-gray-50 rounded">
                        <span className="text-sm text-gray-700">
                          <strong>{item.studentName}</strong> ({item.studentId})
                        </span>
                        <span className={`font-bold ${item.plagiarismScore > 60 ? 'text-red-600' : 'text-green-600'}`}>
                          {item.plagiarismScore.toFixed(1)}%
                        </span>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div className="flex justify-between items-center p-2 bg-gray-50 rounded">
                    <span className="text-sm text-gray-700">
                      <strong>{uploadResult.data.studentName}</strong> ({uploadResult.data.studentId})
                    </span>
                    <span className={`font-bold ${uploadResult.data.plagiarismScore > 60 ? 'text-red-600' : 'text-green-600'}`}>
                      {uploadResult.data.plagiarismScore.toFixed(1)}%
                    </span>
                  </div>
                )}
              </div>
            </div>
          )}

          {!uploadResult && !error && (
            <div className="bg-blue-50 border-2 border-blue-600 rounded-lg p-6">
              <h4 className="font-bold text-blue-900 mb-2">How it works:</h4>
              <ul className="text-sm text-blue-700 space-y-2">
                <li>✓ Upload Flowgorithm files (.fprg, .frpg, .fgl)</li>
                <li>✓ Automatic plagiarism detection</li>
                <li>✓ GPT generation analysis</li>
                <li>✓ Metadata forensics</li>
                <li>✓ Real-time results</li>
              </ul>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default UploadPage;
