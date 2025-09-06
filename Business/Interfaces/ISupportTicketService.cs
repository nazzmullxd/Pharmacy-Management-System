using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Business.DTO;

namespace Business.Interfaces
{
    public interface ISupportTicketService
    {
        Task<IEnumerable<SupportTicketDTO>> GetAllTicketsAsync();
        Task<SupportTicketDTO?> GetTicketByIdAsync(Guid ticketId);
        Task<SupportTicketDTO> CreateTicketAsync(SupportTicketDTO ticketDto);
        Task<SupportTicketDTO> UpdateTicketAsync(SupportTicketDTO ticketDto);
        Task<bool> DeleteTicketAsync(Guid ticketId);
        Task<IEnumerable<SupportTicketDTO>> GetTicketsByStatusAsync(string status);
        Task<IEnumerable<SupportTicketDTO>> GetTicketsByPriorityAsync(string priority);
        Task<IEnumerable<SupportTicketDTO>> GetTicketsByCategoryAsync(string category);
        Task<IEnumerable<SupportTicketDTO>> GetTicketsByUserAsync(Guid userId);
        Task<IEnumerable<SupportTicketDTO>> GetTicketsByAssigneeAsync(Guid assigneeId);
        Task<bool> AssignTicketAsync(Guid ticketId, Guid assigneeId);
        Task<bool> ResolveTicketAsync(Guid ticketId, string resolution);
        Task<bool> CloseTicketAsync(Guid ticketId);
        Task<IEnumerable<SupportTicketDTO>> GetOpenTicketsAsync();
        Task<string> GenerateTicketNumberAsync();
    }
}
