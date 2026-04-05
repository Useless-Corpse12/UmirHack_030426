interface Props extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
}

export default function Input({ label, error, className = '', ...rest }: Props) {
  return (
    <div className="flex flex-col gap-1">
      {label && <label className="text-sm font-medium text-gray-700">{label}</label>}
      <input className={`input ${error ? 'border-red-400 focus:ring-red-300' : ''} ${className}`} {...rest} />
      {error && <p className="text-xs text-red-500">{error}</p>}
    </div>
  )
}
