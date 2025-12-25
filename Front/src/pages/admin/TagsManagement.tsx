import { useState, useEffect, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { tagsApi, type TagDto, type CreateTagRequest, type UpdateTagRequest } from '../../api/catalogApi'

const ITEMS_PER_PAGE = 10

export default function TagsManagement() {
  const { t } = useTranslation()
  const [tags, setTags] = useState<TagDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)

  // Form state
  const [formData, setFormData] = useState<CreateTagRequest>({
    name: '',
    description: ''
  })

  // Pagination
  const totalPages = Math.ceil(tags.length / ITEMS_PER_PAGE)
  const paginatedTags = useMemo(() => {
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    return tags.slice(start, start + ITEMS_PER_PAGE)
  }, [tags, currentPage])

  const fetchTags = async () => {
    setLoading(true)
    setError(null)
    try {
      const response = await tagsApi.getAll()
      if (response.isSuccess) {
        setTags(response.payload || [])
        setCurrentPage(1)
      } else {
        setError(response.message || t('errors.fetch_failed'))
      }
    } catch {
      setError(t('errors.fetch_failed'))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchTags()
  }, [])

  const resetForm = () => {
    setFormData({ name: '', description: '' })
    setEditingId(null)
    setShowCreateForm(false)
  }

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const response = await tagsApi.create(formData)
      if (response.isSuccess) {
        await fetchTags()
        resetForm()
      } else {
        setError(response.message || t('errors.save_failed'))
      }
    } catch {
      setError(t('errors.save_failed'))
    }
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!editingId) return
    try {
      const updateData: UpdateTagRequest = {
        name: formData.name,
        description: formData.description
      }
      const response = await tagsApi.update(editingId, updateData)
      if (response.isSuccess) {
        await fetchTags()
        resetForm()
      } else {
        setError(response.message || t('errors.save_failed'))
      }
    } catch {
      setError(t('errors.save_failed'))
    }
  }

  const handleDelete = async (id: string) => {
    if (!window.confirm(t('admin.catalog.confirm_delete'))) return
    try {
      const response = await tagsApi.delete(id)
      if (response.isSuccess) {
        await fetchTags()
      } else {
        setError(response.message || t('errors.save_failed'))
      }
    } catch {
      setError(t('errors.save_failed'))
    }
  }

  const startEdit = (tag: TagDto) => {
    setFormData({
      name: tag.name,
      description: tag.description || ''
    })
    setEditingId(tag.id)
    setShowCreateForm(true)
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-[200px]">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-brand"></div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold text-text">{t('admin.catalog.tags')}</h2>
        <button
          onClick={() => { resetForm(); setShowCreateForm(true) }}
          className="btn btn-brand"
        >
          {t('admin.catalog.add_tag')}
        </button>
      </div>

      {error && (
        <div className="bg-red-100 dark:bg-red-900/30 border border-red-400 dark:border-red-700 text-red-700 dark:text-red-300 px-4 py-3 rounded">
          {error}
        </div>
      )}

      {/* Create/Edit Form */}
      {showCreateForm && (
        <div className="card p-6">
          <h3 className="text-lg font-semibold text-text mb-4">
            {editingId ? t('admin.catalog.edit_tag') : t('admin.catalog.add_tag')}
          </h3>
          <form onSubmit={editingId ? handleUpdate : handleCreate} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-text mb-1">
                {t('admin.catalog.name')} *
              </label>
              <input
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                className="w-full px-3 py-2 rounded-lg border border-gray-600 bg-surface text-text 
                  focus:outline-none focus:ring-2 focus:ring-brand focus:border-transparent"
                required
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-text mb-1">
                {t('admin.catalog.description')}
              </label>
              <textarea
                value={formData.description || ''}
                onChange={(e) => setFormData({ ...formData, description: e.target.value || null })}
                className="w-full px-3 py-2 rounded-lg border border-gray-600 bg-surface text-text 
                  focus:outline-none focus:ring-2 focus:ring-brand focus:border-transparent resize-y"
                rows={3}
              />
            </div>
            <div className="flex gap-2">
              <button type="submit" className="btn btn-brand">
                {editingId ? t('admin.catalog.save') : t('admin.catalog.create')}
              </button>
              <button type="button" onClick={resetForm} className="btn btn-secondary">
                {t('cancel')}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Tags Table */}
      <div className="card overflow-hidden">
        <table className="min-w-full divide-y divide-border">
          <thead className="bg-surface-secondary">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-text-muted uppercase tracking-wider">
                {t('admin.catalog.name')}
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-text-muted uppercase tracking-wider">
                {t('admin.catalog.slug')}
              </th>
              <th className="px-6 py-3 text-left text-xs font-medium text-text-muted uppercase tracking-wider">
                {t('admin.catalog.description')}
              </th>
              <th className="px-6 py-3 text-right text-xs font-medium text-text-muted uppercase tracking-wider">
                {t('admin.catalog.actions')}
              </th>
            </tr>
          </thead>
          <tbody className="bg-surface divide-y divide-border">
            {paginatedTags.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-6 py-4 text-center text-text-muted">
                  {t('admin.catalog.no_tags')}
                </td>
              </tr>
            ) : (
              paginatedTags.map(tag => (
                <tr key={tag.id} className="hover:bg-surface-secondary/50 transition-colors">
                  <td className="px-6 py-4 whitespace-nowrap text-text font-medium">
                    {tag.name}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-text-muted text-sm">
                    {tag.slug}
                  </td>
                  <td className="px-6 py-4 text-text-muted text-sm max-w-xs truncate">
                    {tag.description || '-'}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm">
                    <button
                      onClick={() => startEdit(tag)}
                      className="text-brand hover:text-brand-light mr-3 transition-colors"
                    >
                      {t('admin.catalog.edit')}
                    </button>
                    <button
                      onClick={() => handleDelete(tag.id)}
                      className="text-red-500 hover:text-red-400 transition-colors"
                    >
                      {t('admin.catalog.delete')}
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* Pagination */}
        {totalPages > 1 && (
          <div className="flex items-center justify-between px-6 py-4 border-t border-border bg-surface-secondary/30">
            <div className="text-sm text-text-muted">
              {t('admin.catalog.showing')} {(currentPage - 1) * ITEMS_PER_PAGE + 1}-
              {Math.min(currentPage * ITEMS_PER_PAGE, tags.length)} {t('admin.catalog.of')} {tags.length}
            </div>
            <div className="flex gap-2">
              <button
                onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                disabled={currentPage === 1}
                className="px-3 py-1 rounded border border-border text-text-muted hover:bg-surface-secondary 
                  disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {t('admin.catalog.prev')}
              </button>
              {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
                <button
                  key={page}
                  onClick={() => setCurrentPage(page)}
                  className={`px-3 py-1 rounded border transition-colors ${
                    currentPage === page
                      ? 'bg-brand text-white border-brand'
                      : 'border-border text-text-muted hover:bg-surface-secondary'
                  }`}
                >
                  {page}
                </button>
              ))}
              <button
                onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                disabled={currentPage === totalPages}
                className="px-3 py-1 rounded border border-border text-text-muted hover:bg-surface-secondary 
                  disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
              >
                {t('admin.catalog.next')}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
