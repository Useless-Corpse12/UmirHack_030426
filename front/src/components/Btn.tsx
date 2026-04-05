interface Props extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'ghost' | 'danger' | 'success'
  loading?: boolean
  size?: 'sm' | 'md'
}

export default function Btn({ variant = 'primary', loading, size = 'md', children, className = '', ...rest }: Props) {
  const base = size === 'sm' ? 'px-3 py-1.5 text-sm' : 'px-5 py-2.5'
  const v = {
    primary: 'btn-primary',
    ghost:   'btn-ghost',
    danger:  'btn-danger',
    success: 'btn-success',
  }[variant]

  return (
    <button className={`${v} ${base} ${className}`} disabled={loading || rest.disabled} {...rest}>
      {loading ? (
        <span className="flex items-center gap-2 justify-center">
          <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"/>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"/>
          </svg>
          {children}
        </span>
      ) : children}
    </button>
  )
}
